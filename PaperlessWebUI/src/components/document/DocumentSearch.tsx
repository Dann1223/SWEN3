import { Input, Select, Button, Space } from 'antd';
import { SearchOutlined, ClearOutlined } from '@ant-design/icons';

const { Search } = Input;
const { Option } = Select;

interface DocumentSearchProps {
  onSearch: (query: string) => void;
  onFilterChange?: (filters: any) => void;
  loading?: boolean;
}

const DocumentSearch = ({ onSearch, onFilterChange, loading }: DocumentSearchProps) => {
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
      
      <Space wrap>
        <Select
          placeholder="File Type"
          style={{ width: 120 }}
          allowClear
          onChange={(value) => onFilterChange?.({ fileType: value })}
        >
          <Option value="pdf">PDF</Option>
          <Option value="doc">Word</Option>
          <Option value="image">Image</Option>
          <Option value="text">Text</Option>
        </Select>

        <Select
          placeholder="Sort By"
          style={{ width: 120 }}
          defaultValue="uploadDate"
          onChange={(value) => onFilterChange?.({ sortBy: value })}
        >
          <Option value="uploadDate">Date</Option>
          <Option value="title">Title</Option>
          <Option value="fileSize">Size</Option>
        </Select>

        <Button 
          icon={<ClearOutlined />}
          onClick={() => onFilterChange?.({})}
        >
          Clear Filters
        </Button>
      </Space>
    </Space>
  );
};

export default DocumentSearch;
