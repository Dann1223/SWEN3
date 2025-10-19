import { Input, Card, List, Typography, Empty, Spin } from 'antd';
import { SearchOutlined, FileTextOutlined } from '@ant-design/icons';
import { useSearch } from '../hooks/useSearch';
import { formatRelativeTime } from '../utils/helpers';

const { Search } = Input;
const { Title } = Typography;

const SearchPage = () => {
  const { 
    filters, 
    isSearching, 
    searchResults, 
    hasActiveSearch,
    updateQuery, 
    performSearch 
  } = useSearch();

  const handleSearch = (value: string) => {
    updateQuery(value);
    if (value.trim()) {
      performSearch({ ...filters, query: value });
    }
  };

  return (
    <div>
      <Title level={2}>Search Documents</Title>
      
      <Card style={{ marginBottom: 24 }}>
        <Search
          placeholder="Search documents by title, content, or tags..."
          allowClear
          enterButton={<SearchOutlined />}
          size="large"
          onSearch={handleSearch}
          defaultValue={filters.query}
        />
      </Card>

      {isSearching && (
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>Searching documents...</div>
        </div>
      )}

      {!isSearching && hasActiveSearch && (
        <Card title={`Search Results (${searchResults.length})`}>
          {searchResults.length === 0 ? (
            <Empty description="No documents found" />
          ) : (
            <List
              dataSource={searchResults}
              renderItem={(doc) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={<FileTextOutlined style={{ fontSize: 20, color: '#1890ff' }} />}
                    title={doc.title}
                    description={
                      <div>
                        <div>{doc.fileName} • {formatRelativeTime(doc.uploadDate)}</div>
                        {doc.summary && (
                          <div style={{ marginTop: 8, color: '#666' }}>
                            {doc.summary.substring(0, 200)}...
                          </div>
                        )}
                      </div>
                    }
                  />
                </List.Item>
              )}
            />
          )}
        </Card>
      )}

      {!hasActiveSearch && !isSearching && (
        <Card>
          <Empty description="Enter a search term to find documents" />
        </Card>
      )}
    </div>
  );
};

export default SearchPage;
