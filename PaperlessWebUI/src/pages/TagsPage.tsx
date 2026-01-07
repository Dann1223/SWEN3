import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Modal,
  Form,
  Input,
  ColorPicker,
  message,
  Popconfirm,
  Space,
  Typography,
  Tag,
  Row,
  Col,
  Statistic
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  TagOutlined
} from '@ant-design/icons';
import { tagService } from '../services/documentService';
import type { Tag as TagType } from '../types/document.types';
import { formatRelativeTime } from '../utils/helpers';

const { Title } = Typography;
const { TextArea } = Input;

const TagsPage: React.FC = () => {
  const [tags, setTags] = useState<TagType[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingTag, setEditingTag] = useState<TagType | null>(null);
  const [form] = Form.useForm();

  useEffect(() => {
    loadTags();
  }, []);

  const loadTags = async () => {
    setLoading(true);
    try {
      const tagsData = await tagService.getAll();
      setTags(tagsData);
    } catch (error) {
      console.error('Failed to load tags:', error);
      message.error('Failed to load tags');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTag = () => {
    setEditingTag(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEditTag = (tag: TagType) => {
    setEditingTag(tag);
    form.setFieldsValue({
      name: tag.name,
      description: tag.description,
      color: tag.color || '#1890ff'
    });
    setModalVisible(true);
  };

  const handleSubmit = async (values: any) => {
    try {
      const tagData = {
        name: values.name,
        description: values.description,
        color: typeof values.color === 'string' ? values.color : values.color?.toHexString?.() || '#1890ff'
      };

      if (editingTag) {
        await tagService.update(editingTag.id, tagData);
        message.success('Tag updated successfully');
      } else {
        await tagService.create(tagData);
        message.success('Tag created successfully');
      }

      setModalVisible(false);
      form.resetFields();
      await loadTags();
    } catch (error) {
      console.error('Failed to save tag:', error);
      message.error('Failed to save tag');
    }
  };

  const handleDeleteTag = async (tagId: number) => {
    try {
      await tagService.delete(tagId);
      message.success('Tag deleted successfully');
      await loadTags();
    } catch (error) {
      console.error('Failed to delete tag:', error);
      message.error('Failed to delete tag');
    }
  };

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: TagType) => (
        <Space>
          <Tag color={record.color || 'default'}>{name}</Tag>
        </Space>
      ),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
    },
    {
      title: 'Color',
      dataIndex: 'color',
      key: 'color',
      width: 100,
      render: (color: string) => (
        <div 
          style={{ 
            width: 20, 
            height: 20, 
            backgroundColor: color || '#1890ff',
            borderRadius: 4,
            border: '1px solid #d9d9d9'
          }} 
        />
      ),
    },
    {
      title: 'Created',
      dataIndex: 'createdDate',
      key: 'createdDate',
      width: 150,
      render: (date: string) => formatRelativeTime(date),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_: any, record: TagType) => (
        <Space>
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => handleEditTag(record)}
          >
            Edit
          </Button>
          <Popconfirm
            title="Are you sure you want to delete this tag?"
            description="This action cannot be undone."
            onConfirm={() => handleDeleteTag(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="text"
              danger
              icon={<DeleteOutlined />}
            >
              Delete
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Row gutter={[24, 24]}>
        <Col span={24}>
          <Card>
            <Row gutter={16}>
              <Col span={6}>
                <Statistic
                  title="Total Tags"
                  value={tags.length}
                  prefix={<TagOutlined />}
                />
              </Col>
            </Row>
          </Card>
        </Col>

        <Col span={24}>
          <Card
            title={<Title level={3}>Tags Management</Title>}
            extra={
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={handleCreateTag}
              >
                Create Tag
              </Button>
            }
          >
            <Table
              columns={columns}
              dataSource={tags}
              rowKey="id"
              loading={loading}
              pagination={{
                total: tags.length,
                pageSize: 10,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} tags`,
              }}
            />
          </Card>
        </Col>
      </Row>

      <Modal
        title={editingTag ? 'Edit Tag' : 'Create Tag'}
        open={modalVisible}
        onOk={() => form.submit()}
        onCancel={() => {
          setModalVisible(false);
          form.resetFields();
        }}
        destroyOnClose
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            name="name"
            label="Tag Name"
            rules={[
              { required: true, message: 'Please enter tag name' },
              { max: 50, message: 'Tag name must be less than 50 characters' }
            ]}
          >
            <Input placeholder="Enter tag name" />
          </Form.Item>

          <Form.Item
            name="description"
            label="Description"
            rules={[
              { max: 200, message: 'Description must be less than 200 characters' }
            ]}
          >
            <TextArea
              rows={3}
              placeholder="Enter tag description (optional)"
            />
          </Form.Item>

          <Form.Item
            name="color"
            label="Color"
            initialValue="#1890ff"
          >
            <ColorPicker showText />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default TagsPage;
