import { Card, Tag, Button, Space, Typography } from 'antd';
import { EyeOutlined, DownloadOutlined, DeleteOutlined } from '@ant-design/icons';
import { formatRelativeTime, formatFileSize, getFileTypeIcon } from '../../utils/helpers';
import type { Document } from '../../types/document.types';

const { Text } = Typography;

interface DocumentCardProps {
  document: Document;
  onView?: (doc: Document) => void;
  onDownload?: (doc: Document) => void;
  onDelete?: (doc: Document) => void;
}

const DocumentCard = ({ document, onView, onDownload, onDelete }: DocumentCardProps) => {
  return (
    <Card
      hoverable
      size="small"
      title={
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{ fontSize: 20 }}>{getFileTypeIcon(document.fileName)}</span>
          <Text strong ellipsis style={{ flex: 1 }}>
            {document.title}
          </Text>
        </div>
      }
      extra={
        <Space>
          <Button type="text" size="small" icon={<EyeOutlined />} onClick={() => onView?.(document)} />
          <Button type="text" size="small" icon={<DownloadOutlined />} onClick={() => onDownload?.(document)} />
          <Button type="text" size="small" danger icon={<DeleteOutlined />} onClick={() => onDelete?.(document)} />
        </Space>
      }
      style={{ height: 200 }}
      bodyStyle={{ padding: 12 }}
    >
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <Text type="secondary" style={{ fontSize: 12 }}>
          {document.fileName}
        </Text>
        
        <div style={{ margin: '8px 0', flex: 1 }}>
          <Space direction="vertical" size={4}>
            <Text style={{ fontSize: 12 }}>
              {formatFileSize(document.fileSize)} • {formatRelativeTime(document.uploadDate)}
            </Text>
            
            <div>
              <Tag color={document.isProcessed ? 'green' : 'orange'} style={{ fontSize: 10 }}>
                {document.isProcessed ? 'Processed' : 'Processing'}
              </Tag>
              <Tag color={document.isIndexed ? 'green' : 'orange'} style={{ fontSize: 10 }}>
                {document.isIndexed ? 'Indexed' : 'Pending'}
              </Tag>
            </div>
          </Space>
        </div>

        {document.summary && (
          <Text style={{ fontSize: 11, color: '#666' }} ellipsis>
            {document.summary.substring(0, 100)}...
          </Text>
        )}
      </div>
    </Card>
  );
};

export default DocumentCard;
