import { useEffect } from 'react';
import { Card, Col, Row, Statistic, List, Typography } from 'antd';
import { FileTextOutlined, ClockCircleOutlined, UploadOutlined } from '@ant-design/icons';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState, AppDispatch } from '../store';
import { fetchDocuments, fetchRecentDocuments } from '../store/slices/documentSlice';
import { formatRelativeTime } from '../utils/helpers';

const { Title } = Typography;

const Dashboard = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { documents, recentDocuments, loading } = useSelector((state: RootState) => state.documents);

  useEffect(() => {
    dispatch(fetchRecentDocuments(5));
    dispatch(fetchDocuments({ page: 1, pageSize: 10 }));
  }, [dispatch]);

  const stats = [
    {
      title: 'Total Documents',
      value: documents.length,
      icon: <FileTextOutlined style={{ color: '#1890ff' }} />,
    },
    {
      title: 'Processed Today',
      value: documents.filter(doc => 
        new Date(doc.uploadDate).toDateString() === new Date().toDateString()
      ).length,
      icon: <UploadOutlined style={{ color: '#52c41a' }} />,
    },
    {
      title: 'Pending OCR',
      value: documents.filter(doc => !doc.isProcessed).length,
      icon: <ClockCircleOutlined style={{ color: '#faad14' }} />,
    },
  ];

  return (
    <div>
      <Title level={2}>Dashboard</Title>
      
      <Row gutter={16} style={{ marginBottom: 24 }}>
        {stats.map((stat, index) => (
          <Col span={8} key={index}>
            <Card>
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
        <Col span={24}>
          <Card title="Recent Documents" loading={loading}>
            <List
              dataSource={recentDocuments}
              renderItem={(doc) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={<FileTextOutlined style={{ fontSize: 20, color: '#1890ff' }} />}
                    title={doc.title}
                    description={`Uploaded ${formatRelativeTime(doc.uploadDate)} • ${doc.fileType}`}
                  />
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default Dashboard;
