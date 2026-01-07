import { useEffect, useState } from 'react';
import { Typography, Space, Button, message } from 'antd';
import { AppstoreOutlined, BarsOutlined, PlusOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState, AppDispatch } from '../store';
import { fetchDocuments, deleteDocument } from '../store/slices/documentSlice';
import { documentService } from '../services/documentService';
import DocumentList from '../components/document/DocumentList';
import DocumentSearch from '../components/document/DocumentSearch';
import IndexManagement from '../components/search/IndexManagement';
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

  const handleDownload = async (doc: Document) => {
    try {
      const blob = await documentService.download(doc.id);
      const url = window.URL.createObjectURL(blob);
      const link = window.document.createElement('a');
      link.href = url;
      link.download = doc.fileName;
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

  const handleView = (doc: Document) => {
    navigate(`/documents/${doc.id}`);
  };

  const handleSearch = (query: string) => {
    // Simple search - just filter by title locally for now
    if (!query.trim()) {
      dispatch(fetchDocuments({ page: 1, pageSize: 10 }));
      return;
    }
    
    // Navigate to advanced search with query
    navigate(`/search?q=${encodeURIComponent(query)}`);
  };

  const handleIndexChanged = () => {
    // Refresh documents when index changes
    dispatch(fetchDocuments({ page: pagination.page, pageSize: pagination.pageSize }));
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

      <IndexManagement onIndexChanged={handleIndexChanged} />

      <div style={{ marginBottom: 24 }}>
        <DocumentSearch
          onSearch={handleSearch}
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
