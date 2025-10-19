import { useEffect, useState } from 'react';
import { Typography, Space, Button, message } from 'antd';
import { AppstoreOutlined, BarsOutlined, PlusOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState, AppDispatch } from '../store';
import { fetchDocuments, deleteDocument } from '../store/slices/documentSlice';
import DocumentList from '../components/document/DocumentList';
import DocumentSearch from '../components/document/DocumentSearch';
import type { Document } from '../types/document.types';

const { Title } = Typography;

const DocumentsPage = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch<AppDispatch>();
  const { documents, loading, pagination } = useSelector((state: RootState) => state.documents);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');

  useEffect(() => {
    dispatch(fetchDocuments({ page: 1, pageSize: 10 }));
  }, [dispatch]);

  const handleDelete = async (doc: Document) => {
    try {
      await dispatch(deleteDocument(doc.id));
      message.success('Document deleted successfully');
    } catch (error) {
      message.error('Failed to delete document');
    }
  };

  const handleDownload = (doc: Document) => {
    message.info(`Download ${doc.fileName} - Feature coming soon`);
  };

  const handleView = (doc: Document) => {
    message.info(`View ${doc.title} - Feature coming soon`);
  };

  const handleSearch = (query: string) => {
    // This will be implemented with the search functionality
    console.log('Search:', query);
  };

  const handleFilterChange = (filters: any) => {
    // This will be implemented with filter functionality
    console.log('Filters:', filters);
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <Title level={2} style={{ margin: 0 }}>Documents</Title>
        
        <Space>
          <Button
            type={viewMode === 'grid' ? 'primary' : 'default'}
            icon={<AppstoreOutlined />}
            onClick={() => setViewMode('grid')}
          />
          <Button
            type={viewMode === 'list' ? 'primary' : 'default'}
            icon={<BarsOutlined />}
            onClick={() => setViewMode('list')}
          />
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => navigate('/upload')}
          >
            Upload
          </Button>
        </Space>
      </div>

      <div style={{ marginBottom: 24 }}>
        <DocumentSearch
          onSearch={handleSearch}
          onFilterChange={handleFilterChange}
          loading={loading}
        />
      </div>

      <DocumentList
        documents={documents}
        loading={loading}
        viewMode={viewMode}
        pagination={{
          current: pagination.page,
          pageSize: pagination.pageSize,
          total: pagination.totalCount,
          onChange: (page, pageSize) => dispatch(fetchDocuments({ page, pageSize })),
        }}
        onView={handleView}
        onDownload={handleDownload}
        onDelete={handleDelete}
      />
    </div>
  );
};

export default DocumentsPage;
