import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Input,
  Card,
  List,
  Typography,
  Empty,
  Spin,
  Row,
  Col,
  Select,
  DatePicker,
  Checkbox,
  Button,
  Tag,
  Collapse,
  message
} from 'antd';
import {
  SearchOutlined,
  FileTextOutlined,
  FilterOutlined,
  StarOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import { searchService } from '../services/searchService';
import type { AdvancedSearchRequest, AdvancedSearchResponse, SearchResult } from '../types/search.types';
import { formatRelativeTime, formatFileSize } from '../utils/helpers';

const { Search } = Input;
const { Title, Text, Paragraph } = Typography;
const { RangePicker } = DatePicker;
const { Option } = Select;
const { Panel } = Collapse;

const AdvancedSearchPage: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [searchResponse, setSearchResponse] = useState<AdvancedSearchResponse | null>(null);
  const [searchRequest, setSearchRequest] = useState<AdvancedSearchRequest>({
    page: 1,
    pageSize: 20,
    enableHighlight: true
  });
  const [suggestions, setSuggestions] = useState<string[]>([]);

  useEffect(() => {
    loadAggregations();
  }, []);

  const loadAggregations = async () => {
    try {
      await searchService.getAggregations();
      // Could store aggregations for showing facets
    } catch (error) {
      console.error('Failed to load aggregations:', error);
    }
  };

  const handleSearch = async (query?: string) => {
    setLoading(true);
    try {
      const request: AdvancedSearchRequest = {
        ...searchRequest,
        query: query || searchRequest.query,
        page: 1
      };

      console.log('Sending search request:', request);
      const response = await searchService.search(request);
      console.log('Received search response:', response);
      
      setSearchResponse(response);
      setSearchRequest(request);

      if (query) {
        try {
          const suggestions = await searchService.getSuggestions(query);
          setSuggestions(suggestions);
        } catch (suggestError) {
          console.error('Failed to get suggestions:', suggestError);
          // Don't fail the whole search if suggestions fail
        }
      }
    } catch (error) {
      console.error('Search failed:', error);
      message.error('Search failed. Please try again.');
      setSearchResponse(null);
    } finally {
      setLoading(false);
    }
  };

  const handleLoadMore = async () => {
    if (!searchResponse || loading) return;

    setLoading(true);
    try {
      const nextPage = searchRequest.page! + 1;
      const request = { ...searchRequest, page: nextPage };
      const response = await searchService.search(request);
      
      setSearchResponse({
        ...response,
        results: [...searchResponse.results, ...response.results]
      });
      setSearchRequest(request);
    } catch (error) {
      console.error('Load more failed:', error);
      message.error('Failed to load more results.');
    } finally {
      setLoading(false);
    }
  };

  const handleSimilarDocuments = async (documentId: number) => {
    try {
      // Navigate to document detail page which shows similar documents
      navigate(`/documents/${documentId}`);
    } catch (error) {
      console.error('Failed to navigate to document:', error);
      message.error('Failed to open document.');
    }
  };

  const renderHighlights = (highlights?: Record<string, string[]>) => {
    if (!highlights) return null;

    return (
      <div style={{ marginTop: 8 }}>
        {Object.entries(highlights).map(([field, values]) => (
          <div key={field} style={{ marginBottom: 4 }}>
            <Text type="secondary">{field}: </Text>
            {values.map((highlight, index) => (
              <span
                key={index}
                dangerouslySetInnerHTML={{ __html: highlight }}
                style={{ backgroundColor: '#fff2e8', padding: '2px 4px', margin: '0 2px' }}
              />
            ))}
          </div>
        ))}
      </div>
    );
  };

  const renderResult = (result: SearchResult) => (
    <List.Item 
      key={result.id}
      style={{ cursor: 'pointer' }}
      onClick={() => navigate(`/documents/${result.id}`)}
    >
      <List.Item.Meta
        avatar={<FileTextOutlined style={{ fontSize: 20, color: '#1890ff' }} />}
        title={
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <span>{result.title}</span>
            {/* <Tag color="green">{result.score > 0 ? result.score.toFixed(2) : '1.23'}</Tag> */}
            {result.confidence && (
              <Tag color="blue">{Math.round(result.confidence * 100)}% confident</Tag>
            )}
          </div>
        }
        description={
          <div>
            <div>
              <Text type="secondary">
                {result.fileName} • {formatFileSize(result.fileSize)} • {formatRelativeTime(result.uploadDate)}
              </Text>
            </div>
            {result.contentSnippet && (
              <Paragraph ellipsis={{ rows: 2 }} style={{ marginTop: 8, marginBottom: 8 }}>
                {result.contentSnippet}
              </Paragraph>
            )}
            {result.tags.length > 0 && (
              <div style={{ marginTop: 8 }}>
                {result.tags.map(tag => (
                  <Tag key={tag}>{tag}</Tag>
                ))}
              </div>
            )}
            {renderHighlights(result.highlights)}
            <div style={{ marginTop: 8 }}>
              <Button
                type="link"
                size="small"
                icon={<StarOutlined />}
                onClick={(e) => {
                  e.stopPropagation();
                  handleSimilarDocuments(result.id);
                }}
              >
                Find Similar
              </Button>
            </div>
          </div>
        }
      />
    </List.Item>
  );

  return (
    <div>
      <Title level={2}>Advanced Search</Title>

      <Card style={{ marginBottom: 24 }}>
        <Row gutter={[16, 16]}>
          <Col span={24}>
            <Search
              placeholder="Search documents by content, title, or metadata..."
              allowClear
              enterButton={<SearchOutlined />}
              size="large"
              onSearch={handleSearch}
              defaultValue={searchRequest.query}
            />
          </Col>
        </Row>

        <Collapse 
          style={{ marginTop: 16 }}
          expandIconPosition="right"
          ghost
        >
          <Panel header="Advanced Filters" key="filters" extra={<FilterOutlined />}>
            <Row gutter={[16, 16]}>
              <Col md={12} xs={24}>
                <Text strong>File Types:</Text>
                <Select
                  mode="multiple"
                  placeholder="Select file types"
                  style={{ width: '100%', marginTop: 8 }}
                  value={searchRequest.fileTypes}
                  onChange={(fileTypes) => setSearchRequest({ ...searchRequest, fileTypes })}
                >
                  <Option value="pdf">PDF</Option>
                  <Option value="jpg">JPG</Option>
                  <Option value="png">PNG</Option>
                  <Option value="tiff">TIFF</Option>
                  <Option value="doc">DOC</Option>
                  <Option value="docx">DOCX</Option>
                </Select>
              </Col>

              <Col md={12} xs={24}>
                <Text strong>Date Range:</Text>
                <RangePicker
                  style={{ width: '100%', marginTop: 8 }}
                  onChange={(dates) => {
                    setSearchRequest({
                      ...searchRequest,
                      dateFrom: dates?.[0]?.toISOString(),
                      dateTo: dates?.[1]?.toISOString()
                    });
                  }}
                />
              </Col>

              <Col span={24}>
                <Checkbox
                  checked={searchRequest.processedOnly}
                  onChange={(e) => setSearchRequest({ ...searchRequest, processedOnly: e.target.checked })}
                >
                  Only processed documents
                </Checkbox>
                <Checkbox
                  checked={searchRequest.enableHighlight}
                  onChange={(e) => setSearchRequest({ ...searchRequest, enableHighlight: e.target.checked })}
                  style={{ marginLeft: 16 }}
                >
                  Enable highlights
                </Checkbox>
              </Col>

              <Col span={24}>
                <Button
                  type="primary"
                  icon={<SearchOutlined />}
                  onClick={() => handleSearch()}
                  loading={loading}
                >
                  Apply Filters
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  onClick={() => setSearchRequest({ page: 1, pageSize: 20, enableHighlight: true })}
                  style={{ marginLeft: 8 }}
                >
                  Reset
                </Button>
              </Col>
            </Row>
          </Panel>
        </Collapse>
      </Card>

      {suggestions.length > 0 && (
        <Card title="Suggestions" size="small" style={{ marginBottom: 16 }}>
          {suggestions.map((suggestion, index) => (
            <Tag
              key={index}
              style={{ cursor: 'pointer', marginBottom: 4 }}
              onClick={() => handleSearch(suggestion)}
            >
              {suggestion}
            </Tag>
          ))}
        </Card>
      )}

      {loading && !searchResponse && (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>Searching documents...</div>
        </div>
      )}

      {searchResponse && (
        <Card 
          title={
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span>
                Search Results ({searchResponse.totalCount.toLocaleString()})
              </span>
              <Text type="secondary">
                {searchResponse.executionTime}ms
              </Text>
            </div>
          }
        >
          {searchResponse.results.length === 0 ? (
            <Empty description="No documents found" />
          ) : (
            <>
              <List
                dataSource={searchResponse.results}
                renderItem={renderResult}
              />
              
              {searchResponse.hasMore && (
                <div style={{ textAlign: 'center', marginTop: 16 }}>
                  <Button
                    type="primary"
                    loading={loading}
                    onClick={handleLoadMore}
                  >
                    Load More ({searchResponse.totalCount - searchResponse.results.length} remaining)
                  </Button>
                </div>
              )}
            </>
          )}
        </Card>
      )}
    </div>
  );
};

export default AdvancedSearchPage;
