import { useEffect, useState } from 'react';
import { Card, Col, Row, Statistic, List, Typography, Button, Space, Tag } from 'antd';
import { 
  FileTextOutlined, 
  UploadOutlined, 
  SearchOutlined,
  TagOutlined,
  MessageOutlined,
  EyeOutlined
} from '@ant-design/icons';
import { useSelector, useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import type { RootState, AppDispatch } from '../store';
import { fetchDocuments, fetchRecentDocuments } from '../store/slices/documentSlice';
import { tagService } from '../services/documentService';
import { commentService } from '../services/commentService';
import type { Tag as TagType } from '../types/document.types';
import type { Comment } from '../types/comment.types';
import { formatRelativeTime } from '../utils/helpers';

const { Title } = Typography;

const Dashboard = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { documents, recentDocuments, loading } = useSelector((state: RootState) => state.documents);
  const [tags, setTags] = useState<TagType[]>([]);
  const [recentComments, setRecentComments] = useState<Comment[]>([]);

  useEffect(() => {
    dispatch(fetchRecentDocuments(5));
    dispatch(fetchDocuments({ page: 1, pageSize: 10 }));
    loadTags();
    loadRecentComments();
  }, [dispatch]);

  const loadTags = async () => {
    try {
      const tagsData = await tagService.getAll();
      setTags(tagsData);
    } catch (error) {
      console.error('Failed to load tags:', error);
    }
  };

  const loadRecentComments = async () => {
    try {
      const commentsData = await commentService.getRecent(5);
      setRecentComments(commentsData);
    } catch (error) {
      console.error('Failed to load recent comments:', error);
    }
  };

  useEffect(() => {
    dispatch(fetchRecentDocuments(5));
    dispatch(fetchDocuments({ page: 1, pageSize: 10 }));
  }, [dispatch]);

  const stats = [
    {
      title: 'Total Documents',
      value: documents.length,
      icon: <FileTextOutlined style={{ color: '#1890ff' }} />,
      action: () => navigate('/documents'),
    },
    {
      title: 'Processed Today',
      value: documents.filter(doc => 
        new Date(doc.uploadDate).toDateString() === new Date().toDateString()
      ).length,
      icon: <UploadOutlined style={{ color: '#52c41a' }} />,
      action: () => navigate('/documents'),
    },
    {
      title: 'Total Tags',
      value: tags.length,
      icon: <TagOutlined style={{ color: '#722ed1' }} />,
      action: () => navigate('/tags'),
    },
    {
      title: 'Recent Comments',
      value: recentComments.length,
      icon: <MessageOutlined style={{ color: '#faad14' }} />,
      action: () => navigate('/documents'),
    },
  ];

  return (
    <div>
      <Title level={2}>Dashboard</Title>
      
      <Row gutter={16} style={{ marginBottom: 24 }}>
        {stats.map((stat, index) => (
          <Col span={6} key={index}>
            <Card hoverable onClick={stat.action} style={{ cursor: 'pointer' }}>
              <Statistic
                title={stat.title}
                value={stat.value}
                prefix={stat.icon}
              />
            </Card>
          </Col>
        ))}
      </Row>

      <Row gutter={16}>
        <Col span={12}>
          <Card 
            title="Recent Documents" 
            loading={loading}
            extra={
              <Button 
                type="link" 
                icon={<EyeOutlined />}
                onClick={() => navigate('/documents')}
              >
                View All
              </Button>
            }
          >
            <List
              dataSource={recentDocuments}
              renderItem={(doc) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={<FileTextOutlined style={{ fontSize: 20, color: '#1890ff' }} />}
                    title={
                      <Button 
                        type="link" 
                        onClick={() => navigate(`/documents/${doc.id}`)}
                        style={{ padding: 0, height: 'auto' }}
                      >
                        {doc.title}
                      </Button>
                    }
                    description={
                      <Space>
                        <span>Uploaded {formatRelativeTime(doc.uploadDate)}</span>
                        <Tag>{doc.fileType}</Tag>
                        {doc.isProcessed ? (
                          <Tag color="success">Processed</Tag>
                        ) : (
                          <Tag color="warning">Processing</Tag>
                        )}
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </Card>
        </Col>

        <Col span={12}>
          <Card 
            title="Recent Comments"
            extra={
              <Button 
                type="link" 
                icon={<EyeOutlined />}
                onClick={() => navigate('/documents')}
              >
                View All
              </Button>
            }
          >
            <List
              dataSource={recentComments}
              renderItem={(comment) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={<MessageOutlined style={{ fontSize: 20, color: '#faad14' }} />}
                    title={
                      <Button 
                        type="link" 
                        onClick={() => navigate(`/documents/${comment.documentId}`)}
                        style={{ padding: 0, height: 'auto' }}
                      >
                        {comment.authorName}
                      </Button>
                    }
                    description={
                      <div>
                        <div>{comment.content.substring(0, 100)}...</div>
                        <Space style={{ marginTop: 4 }}>
                          <span>{formatRelativeTime(comment.createdAt)}</span>
                          {comment.isEdited && <Tag>Edited</Tag>}
                        </Space>
                      </div>
                    }
                  />
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={16} style={{ marginTop: 16 }}>
        <Col span={24}>
          <Card title="Quick Actions">
            <Space size="middle">
              <Button 
                type="primary" 
                icon={<UploadOutlined />}
                onClick={() => navigate('/upload')}
              >
                Upload Document
              </Button>
              <Button 
                icon={<SearchOutlined />}
                onClick={() => navigate('/advanced-search')}
              >
                Advanced Search
              </Button>
              <Button 
                icon={<TagOutlined />}
                onClick={() => navigate('/tags')}
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

export default Dashboard;
