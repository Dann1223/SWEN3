import { useState } from 'react';
import { Button, Card, Space, Typography, Popconfirm, message, Tooltip } from 'antd';
import { 
  DatabaseOutlined, 
  ReloadOutlined, 
  SyncOutlined,
  InfoCircleOutlined
} from '@ant-design/icons';
import { searchService } from '../../services/searchService';

const { Text } = Typography;

interface IndexManagementProps {
  onIndexChanged?: () => void;
}

const IndexManagement = ({ onIndexChanged }: IndexManagementProps) => {
  const [loading, setLoading] = useState({
    create: false,
    rebuild: false,
    delete: false,
    sync: false,
    health: false
  });

  const [healthStatus, setHealthStatus] = useState<any>(null);

  const setLoadingState = (key: keyof typeof loading, value: boolean) => {
    setLoading(prev => ({ ...prev, [key]: value }));
  };

  const handleCreateIndex = async () => {
    setLoadingState('create', true);
    try {
      const result = await searchService.createIndex();
      if (result.success) {
        message.success(result.message);
        onIndexChanged?.();
      } else {
        message.error(result.message);
      }
    } catch (error) {
      console.error('Failed to create index:', error);
      message.error('Failed to create search index');
    } finally {
      setLoadingState('create', false);
    }
  };

  const handleRebuildIndex = async () => {
    setLoadingState('rebuild', true);
    try {
      const result = await searchService.rebuildIndex();
      if (result.success) {
        message.success(`${result.message} (${result.count} documents)`);
        onIndexChanged?.();
      } else {
        message.error(result.message);
      }
    } catch (error) {
      console.error('Failed to rebuild index:', error);
      message.error('Failed to rebuild search index');
    } finally {
      setLoadingState('rebuild', false);
    }
  };

  const handleSyncStatus = async () => {
    setLoadingState('sync', true);
    try {
      const result = await searchService.syncIndexingStatus();
      if (result.success) {
        message.success(`${result.message} (${result.count} documents)`);
        onIndexChanged?.();
      } else {
        message.error(result.message);
      }
    } catch (error) {
      console.error('Failed to sync indexing status:', error);
      message.error('Failed to sync indexing status');
    } finally {
      setLoadingState('sync', false);
    }
  };

  const handleCheckHealth = async () => {
    setLoadingState('health', true);
    try {
      const result = await searchService.checkHealth();
      setHealthStatus(result);
      if (result.status === 'Healthy') {
        message.success('Search service is healthy');
      } else {
        message.warning('Search service is not healthy');
      }
    } catch (error) {
      console.error('Failed to check health:', error);
      message.error('Failed to check search service health');
      setHealthStatus({ status: 'Error', error: 'Health check failed' });
    } finally {
      setLoadingState('health', false);
    }
  };

  return (
    <Card 
      title={
        <Space>
          <DatabaseOutlined />
          <span>Search Index Management</span>
          <Tooltip title="Manage Elasticsearch search index for better document search performance">
            <InfoCircleOutlined style={{ color: '#999' }} />
          </Tooltip>
        </Space>
      }
      size="small"
      style={{ marginBottom: 16 }}
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <Text strong>Search Service Status</Text>
            {healthStatus && (
              <div style={{ marginTop: 4 }}>
                <Text 
                  type={healthStatus.status === 'Healthy' ? 'success' : 'danger'}
                  style={{ fontSize: 12 }}
                >
                  {healthStatus.status} - {healthStatus.service}
                </Text>
                {healthStatus.timestamp && (
                  <Text type="secondary" style={{ fontSize: 11, marginLeft: 8 }}>
                    {new Date(healthStatus.timestamp).toLocaleString()}
                  </Text>
                )}
              </div>
            )}
          </div>
          <Button
            size="small"
            icon={<ReloadOutlined />}
            loading={loading.health}
            onClick={handleCheckHealth}
          >
            Check Health
          </Button>
        </div>

        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
          <Button
            size="small"
            icon={<DatabaseOutlined />}
            loading={loading.create}
            onClick={handleCreateIndex}
          >
            Create Index
          </Button>

          <Popconfirm
            title="Rebuild Search Index"
            description="This will delete and recreate the search index. All documents will be reindexed. Continue?"
            onConfirm={handleRebuildIndex}
            okText="Yes"
            cancelText="No"
          >
            <Button
              size="small"
              icon={<ReloadOutlined />}
              loading={loading.rebuild}
            >
              Rebuild Index
            </Button>
          </Popconfirm>

          <Button
            size="small"
            icon={<SyncOutlined />}
            loading={loading.sync}
            onClick={handleSyncStatus}
          >
            Sync Status
          </Button>
{/* 
          <Popconfirm
            title="Delete Search Index"
            description="This will permanently delete the search index. Search functionality will be unavailable until index is recreated. Continue?"
            onConfirm={handleDeleteIndex}
            okText="Yes"
            cancelText="No"
          >
            <Button
              size="small"
              danger
              icon={<DeleteOutlined />}
              loading={loading.delete}
            >
              Delete Index
            </Button>
          </Popconfirm> */}
        </div>
      </Space>
    </Card>
  );
};

export default IndexManagement;
