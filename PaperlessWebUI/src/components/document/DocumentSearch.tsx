import { Input, Button, Space } from 'antd';
import { SearchOutlined } from '@ant-design/icons';

const { Search } = Input;

interface DocumentSearchProps {
  onSearch: (query: string) => void;
  loading?: boolean;
}

const DocumentSearch = ({ onSearch, loading }: DocumentSearchProps) => {
  return (
    <Space direction="vertical" size="middle" style={{ width: '100%' }}>
      <Search
        placeholder="Search documents by title, content, or tags..."
        allowClear
        enterButton={<SearchOutlined />}
        size="large"
        onSearch={onSearch}
        loading={loading}
      />
      
      <div style={{ textAlign: 'center' }}>
        <Button 
          type="link" 
          onClick={() => window.open('/search', '_blank')}
        >
          Advanced Search
        </Button>
      </div>
    </Space>
  );
};

export default DocumentSearch;
