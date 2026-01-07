import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Card,
  Typography,
  Button,
  Space,
  Descriptions,
  Tag,
  Spin,
  message,
  Row,
  Col,
  Tabs,
  Alert,
  Progress
} from 'antd';
import {
  ArrowLeftOutlined,
  DownloadOutlined,
  MessageOutlined,
  SearchOutlined,
  FileTextOutlined,
  TagOutlined
} from '@ant-design/icons';
import { documentService } from '../services/documentService';
import { searchService } from '../services/searchService';
import DocumentComments from '../components/comments/DocumentComments';
import type { Document } from '../types/document.types';
import type { SearchResult } from '../types/search.types';
import { formatRelativeTime, formatFileSize } from '../utils/helpers';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;

const DocumentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [document, setDocument] = useState<Document | null>(null);
  const [loading, setLoading] = useState(true);
  const [ocrStatus, setOcrStatus] = useState<any>(null);
  const [ocrText, setOcrText] = useState<string>('');
  const [similarDocuments, setSimilarDocuments] = useState<SearchResult[]>([]);
  const [activeTab, setActiveTab] = useState('details');

  useEffect(() => {
    if (id) {
      loadDocument(parseInt(id));
      loadOcrStatus(parseInt(id));
      loadSimilarDocuments(parseInt(id));
    }
  }, [id]);

  const loadDocument = async (documentId: number) => {
    setLoading(true);
    try {
      const doc = await documentService.getById(documentId);
      setDocument(doc);
    } catch (error) {
      console.error('Failed to load document:', error);
      message.error('Failed to load document');
      navigate('/documents');
    } finally {
      setLoading(false);
    }
  };

  const loadOcrStatus = async (documentId: number) => {
    try {
      const status = await documentService.getOcrStatus(documentId);
      setOcrStatus(status);
      
      if (status.hasOcrText) {
        const ocrData = await documentService.getOcrText(documentId);
        setOcrText(ocrData.text || '');
      }
    } catch (error) {
      console.error('Failed to load OCR status:', error);
    }
  };

  const loadSimilarDocuments = async (documentId: number) => {
    try {
      const similar = await searchService.getSimilarDocuments(documentId, 5);
      setSimilarDocuments(similar);
    } catch (error) {
      console.error('Failed to load similar documents:', error);
    }
  };

  const handleDownload = async () => {
    if (!document) return;
    
    try {
      const blob = await documentService.download(document.id);
      const url = window.URL.createObjectURL(blob);
      const link = window.document.createElement('a');
      link.href = url;
      link.download = document.fileName;
      window.document.body.appendChild(link);
      link.click();
      window.URL.revokeObjectURL(url);
      window.document.body.removeChild(link);
      message.success('Download started');
    } catch (error) {
      console.error('Download failed:', error);
      message.error('Download failed');
    }
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>Loading document...</div>
      </div>
    );
  }

  if (!document) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Title level={3}>Document not found</Title>
        <Button type="primary" onClick={() => navigate('/documents')}>
          Back to Documents
        </Button>
      </div>
    );
  }

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <Space style={{ marginBottom: 16 }}>
          <Button
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate('/documents')}
          >
            Back to Documents
          </Button>
          <Button
            type="primary"
            icon={<DownloadOutlined />}
            onClick={handleDownload}
          >
            Download
          </Button>
        </Space>

        <Title level={2}>{document.title}</Title>
        <Space wrap style={{ marginBottom: 16 }}>
          {document.tags?.map(tag => (
            <Tag key={tag.id} color={tag.color || 'default'}>
              <TagOutlined /> {tag.name}
            </Tag>
          ))}
        </Space>
      </Card>

      <Row gutter={[24, 24]}>
        <Col lg={16} xs={24}>
          <Card>
            <Tabs activeKey={activeTab} onChange={setActiveTab}>
              <TabPane tab="Details" key="details">
                <Descriptions column={1} bordered>
                  <Descriptions.Item label="File Name">
                    {document.fileName}
                  </Descriptions.Item>
                  <Descriptions.Item label="File Type">
                    <Tag>{document.fileType.toUpperCase()}</Tag>
                  </Descriptions.Item>
                  <Descriptions.Item label="File Size">
                    {formatFileSize(document.fileSize)}
                  </Descriptions.Item>
                  <Descriptions.Item label="Upload Date">
                    {formatRelativeTime(document.uploadDate)}
                  </Descriptions.Item>
                  <Descriptions.Item label="Last Modified">
                    {document.lastModified ? formatRelativeTime(document.lastModified) : 'Never'}
                  </Descriptions.Item>
                  <Descriptions.Item label="Processing Status">
                    <Space>
                      <Tag color={document.isProcessed ? 'success' : 'warning'}>
                        {document.isProcessed ? 'Processed' : 'Pending'}
                      </Tag>
                      <Tag color={document.isIndexed ? 'success' : 'warning'}>
                        {document.isIndexed ? 'Indexed' : 'Not Indexed'}
                      </Tag>
                    </Space>
                  </Descriptions.Item>
                </Descriptions>

                {document.summary && (
                  <div style={{ marginTop: 16 }}>
                    <Title level={4}>Summary</Title>
                    <Paragraph>{document.summary}</Paragraph>
                  </div>
                )}
              </TabPane>

              <TabPane tab="OCR Text" key="ocr">
                {ocrStatus && (
                  <div style={{ marginBottom: 16 }}>
                    <Alert
                      message={`OCR Status: ${ocrStatus.isProcessed ? 'Processed' : 'Pending'}`}
                      description={
                        <div>
                          {ocrStatus.confidence && (
                            <div style={{ marginBottom: 8 }}>
                              <Text>Confidence: </Text>
                              <Progress
                                percent={Math.round(ocrStatus.confidence * 100)}
                                size="small"
                                style={{ width: 200, display: 'inline-block', marginLeft: 8 }}
                              />
                            </div>
                          )}
                          {ocrStatus.processedAt && (
                            <div>
                              <Text>Processed: {formatRelativeTime(ocrStatus.processedAt)}</Text>
                            </div>
                          )}
                          {ocrStatus.processingMethod && (
                            <div>
                              <Text>Method: {ocrStatus.processingMethod}</Text>
                            </div>
                          )}
                        </div>
                      }
                      type={ocrStatus.isProcessed ? 'success' : 'warning'}
                      showIcon
                    />
                  </div>
                )}

                {ocrText ? (
                  <Card size="small">
                    <pre style={{ whiteSpace: 'pre-wrap', margin: 0 }}>
                      {ocrText}
                    </pre>
                  </Card>
                ) : (
                  <div style={{ textAlign: 'center', padding: '40px' }}>
                    <FileTextOutlined style={{ fontSize: 48, color: '#d9d9d9' }} />
                    <div style={{ marginTop: 16 }}>
                      <Text type="secondary">
                        {ocrStatus?.isProcessed 
                          ? 'No OCR text available' 
                          : 'OCR processing in progress...'}
                      </Text>
                    </div>
                  </div>
                )}
              </TabPane>

              <TabPane tab={`Comments`} key="comments">
                <DocumentComments documentId={document.id} />
              </TabPane>
            </Tabs>
          </Card>
        </Col>

        <Col lg={8} xs={24}>
          <Card title="Similar Documents" size="small">
            {similarDocuments.length > 0 ? (
              <Space direction="vertical" style={{ width: '100%' }}>
                {similarDocuments.map((similar) => (
                  <Card
                    key={similar.id}
                    size="small"
                    hoverable
                    onClick={() => navigate(`/documents/${similar.id}`)}
                    style={{ cursor: 'pointer' }}
                  >
                    <Card.Meta
                      title={
                        <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                          <span style={{ fontSize: 14 }}>{similar.title}</span>
                          <Tag color="blue">
                            {similar.score.toFixed(2)}
                          </Tag>
                        </div>
                      }
                      description={
                        <div>
                          <Text type="secondary" style={{ fontSize: 12 }}>
                            {similar.fileName}
                          </Text>
                          {similar.contentSnippet && (
                            <Paragraph
                              ellipsis={{ rows: 2 }}
                              style={{ fontSize: 12, marginTop: 4, marginBottom: 0 }}
                            >
                              {similar.contentSnippet}
                            </Paragraph>
                          )}
                        </div>
                      }
                    />
                  </Card>
                ))}
                <Button
                  type="link"
                  icon={<SearchOutlined />}
                  onClick={() => navigate(`/search?similar=${document.id}`)}
                  style={{ padding: 0 }}
                >
                  View all similar documents
                </Button>
              </Space>
            ) : (
              <div style={{ textAlign: 'center', padding: '20px' }}>
                <Text type="secondary">No similar documents found</Text>
              </div>
            )}
          </Card>

          <Card title="Quick Actions" size="small" style={{ marginTop: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button
                block
                icon={<MessageOutlined />}
                onClick={() => setActiveTab('comments')}
              >
                Add Comment
              </Button>
              <Button
                block
                icon={<SearchOutlined />}
                onClick={() => navigate(`/search?q=${encodeURIComponent(document.title)}`)}
              >
                Search Similar
              </Button>
              <Button
                block
                icon={<TagOutlined />}
                onClick={() => message.info('Tag management coming soon')}
              >
                Manage Tags
              </Button>
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default DocumentDetailPage;
