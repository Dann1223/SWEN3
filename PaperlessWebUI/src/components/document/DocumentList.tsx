import { Row, Col, Empty, Pagination } from 'antd';
import DocumentCard from './DocumentCard';
import LoadingSpinner from '../common/LoadingSpinner';
import type { Document } from '../../types/document.types';

interface DocumentListProps {
  documents: Document[];
  loading?: boolean;
  viewMode?: 'grid' | 'list';
  pagination?: {
    current: number;
    pageSize: number;
    total: number;
    onChange: (page: number, pageSize: number) => void;
  };
  onView?: (doc: Document) => void;
  onDownload?: (doc: Document) => void;
  onDelete?: (doc: Document) => void;
}

const DocumentList = ({ 
  documents, 
  loading, 
  viewMode = 'grid',
  pagination,
  onView, 
  onDownload, 
  onDelete 
}: DocumentListProps) => {
  if (loading) {
    return <LoadingSpinner tip="Loading documents..." />;
  }

  if (documents.length === 0) {
    return <Empty description="No documents found" />;
  }

  return (
    <div>
      {viewMode === 'grid' ? (
        <Row gutter={[16, 16]}>
          {documents.map((doc) => (
            <Col xs={24} sm={12} md={8} lg={6} key={doc.id}>
              <DocumentCard
                document={doc}
                onView={onView}
                onDownload={onDownload}
                onDelete={onDelete}
              />
            </Col>
          ))}
        </Row>
      ) : (
        // List view would be implemented here with Table component
        <div>List view not implemented yet</div>
      )}

      {pagination && (
        <div style={{ textAlign: 'center', marginTop: 24 }}>
          <Pagination
            current={pagination.current}
            pageSize={pagination.pageSize}
            total={pagination.total}
            onChange={pagination.onChange}
            showSizeChanger
            showQuickJumper
            showTotal={(total, range) =>
              `${range[0]}-${range[1]} of ${total} documents`
            }
          />
        </div>
      )}
    </div>
  );
};

export default DocumentList;
