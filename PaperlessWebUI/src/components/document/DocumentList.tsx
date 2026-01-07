import { Row, Col, Empty, Pagination, Table, Tag, Button, Space, Typography } from 'antd';
import { EyeOutlined, DownloadOutlined, DeleteOutlined } from '@ant-design/icons';
import DocumentCard from './DocumentCard';
import LoadingSpinner from '../common/LoadingSpinner';
import { formatRelativeTime, formatFileSize, getFileTypeIcon } from '../../utils/helpers';
import type { Document } from '../../types/document.types';

const { Text } = Typography;

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
        <Table
          dataSource={documents}
          rowKey="id"
          pagination={false}
          columns={[
            {
              title: 'Document',
              dataIndex: 'title',
              key: 'title',
              render: (title, record) => (
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <span style={{ fontSize: 16 }}>{getFileTypeIcon(record.fileName)}</span>
                  <div>
                    <div>
                      <Text strong>{title}</Text>
                    </div>
                    <div>
                      <Text type="secondary" style={{ fontSize: 12 }}>
                        {record.fileName}
                      </Text>
                    </div>
                  </div>
                </div>
              ),
            },
            {
              title: 'Size',
              dataIndex: 'fileSize',
              key: 'fileSize',
              width: 100,
              render: (size) => formatFileSize(size),
            },
            {
              title: 'Status',
              key: 'status',
              width: 150,
              render: (_, record) => (
                <Space direction="vertical" size={2}>
                  <Tag color={record.isProcessed ? 'green' : 'orange'} style={{ fontSize: 10 }}>
                    {record.isProcessed ? 'Processed' : 'Processing'}
                  </Tag>
                  <Tag color={record.isIndexed ? 'green' : 'orange'} style={{ fontSize: 10 }}>
                    {record.isIndexed ? 'Indexed' : 'Pending'}
                  </Tag>
                </Space>
              ),
            },
            {
              title: 'Upload Date',
              dataIndex: 'uploadDate',
              key: 'uploadDate',
              width: 120,
              render: (date) => formatRelativeTime(date),
            },
            {
              title: 'Actions',
              key: 'actions',
              width: 120,
              render: (_, record) => (
                <Space>
                  <Button 
                    type="text" 
                    size="small" 
                    icon={<EyeOutlined />} 
                    onClick={() => onView?.(record)}
                    title="View"
                  />
                  <Button 
                    type="text" 
                    size="small" 
                    icon={<DownloadOutlined />} 
                    onClick={() => onDownload?.(record)}
                    title="Download"
                  />
                  <Button 
                    type="text" 
                    size="small" 
                    danger 
                    icon={<DeleteOutlined />} 
                    onClick={() => onDelete?.(record)}
                    title="Delete"
                  />
                </Space>
              ),
            },
          ]}
        />
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
