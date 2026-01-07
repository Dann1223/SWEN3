import React, { useState, useEffect } from 'react';
import {
  Card,
  List,
  Typography,
  Button,
  Input,
  Form,
  Avatar,
  Space,
  message,
  Popconfirm,
  Divider,
  Badge,
  Tag
} from 'antd';
import {
  UserOutlined,
  EditOutlined,
  DeleteOutlined,
  MessageOutlined,
  SendOutlined
} from '@ant-design/icons';
import { commentService } from '../../services/commentService';
import type { Comment, CreateCommentRequest, UpdateCommentRequest } from '../../types/comment.types';
import { formatRelativeTime } from '../../utils/helpers';

const { TextArea } = Input;
const { Text } = Typography;

interface DocumentCommentsProps {
  documentId: number;
}

const DocumentComments: React.FC<DocumentCommentsProps> = ({ documentId }) => {
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [editingCommentId, setEditingCommentId] = useState<number | null>(null);
  const [replyingToId, setReplyingToId] = useState<number | null>(null);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();

  useEffect(() => {
    loadComments();
  }, [documentId]);

  const loadComments = async () => {
    setLoading(true);
    try {
      const commentsData = await commentService.getByDocumentId(documentId, true);
      setComments(commentsData);
    } catch (error) {
      console.error('Failed to load comments:', error);
      message.error('Failed to load comments');
    } finally {
      setLoading(false);
    }
  };

  const handleAddComment = async (values: any) => {
    setSubmitting(true);
    try {
      const newComment: CreateCommentRequest = {
        documentId,
        authorName: values.authorName,
        content: values.content,
        parentCommentId: replyingToId || undefined,
        position: values.position
      };

      await commentService.create(newComment);
      form.resetFields();
      setReplyingToId(null);
      await loadComments();
      message.success('Comment added successfully');
    } catch (error) {
      console.error('Failed to add comment:', error);
      message.error('Failed to add comment');
    } finally {
      setSubmitting(false);
    }
  };

  const handleEditComment = async (commentId: number, values: any) => {
    try {
      const updateData: UpdateCommentRequest = {
        content: values.content
      };

      await commentService.update(commentId, updateData);
      setEditingCommentId(null);
      editForm.resetFields();
      await loadComments();
      message.success('Comment updated successfully');
    } catch (error) {
      console.error('Failed to update comment:', error);
      message.error('Failed to update comment');
    }
  };

  const handleDeleteComment = async (commentId: number) => {
    try {
      await commentService.delete(commentId);
      await loadComments();
      message.success('Comment deleted successfully');
    } catch (error) {
      console.error('Failed to delete comment:', error);
      message.error('Failed to delete comment');
    }
  };

  const renderComment = (comment: Comment, isReply = false) => (
    <div key={comment.id} style={{ marginLeft: isReply ? 40 : 0, marginBottom: 16 }}>
      <List.Item>
        <List.Item.Meta
          avatar={<Avatar icon={<UserOutlined />} />}
          title={
            <Space>
              <Text strong>{comment.authorName}</Text>
              <Text type="secondary">{formatRelativeTime(comment.createdAt)}</Text>
              {comment.isEdited && <Tag>Edited</Tag>}
              {comment.replyCount > 0 && (
                <Badge count={comment.replyCount} size="small" />
              )}
            </Space>
          }
          description={
            <div>
              {editingCommentId === comment.id ? (
                <Form
                  form={editForm}
                  onFinish={(values) => handleEditComment(comment.id, values)}
                  initialValues={{ content: comment.content }}
                >
                  <Form.Item
                    name="content"
                    rules={[{ required: true, message: 'Please enter content' }]}
                  >
                    <TextArea rows={3} placeholder="Edit your comment..." />
                  </Form.Item>
                  <Form.Item>
                    <Space>
                      <Button
                        type="primary"
                        htmlType="submit"
                        size="small"
                        icon={<SendOutlined />}
                      >
                        Save
                      </Button>
                      <Button
                        size="small"
                        onClick={() => {
                          setEditingCommentId(null);
                          editForm.resetFields();
                        }}
                      >
                        Cancel
                      </Button>
                    </Space>
                  </Form.Item>
                </Form>
              ) : (
                <div>
                  <Text>{comment.content}</Text>
                  <div style={{ marginTop: 8 }}>
                    <Space size="small">
                      <Button
                        type="link"
                        size="small"
                        icon={<MessageOutlined />}
                        onClick={() => setReplyingToId(comment.id)}
                      >
                        Reply
                      </Button>
                      <Button
                        type="link"
                        size="small"
                        icon={<EditOutlined />}
                        onClick={() => {
                          setEditingCommentId(comment.id);
                          editForm.setFieldsValue({ content: comment.content });
                        }}
                      >
                        Edit
                      </Button>
                      <Popconfirm
                        title="Are you sure you want to delete this comment?"
                        onConfirm={() => handleDeleteComment(comment.id)}
                        okText="Yes"
                        cancelText="No"
                      >
                        <Button
                          type="link"
                          size="small"
                          danger
                          icon={<DeleteOutlined />}
                        >
                          Delete
                        </Button>
                      </Popconfirm>
                    </Space>
                  </div>
                </div>
              )}

              {replyingToId === comment.id && (
                <div style={{ marginTop: 16, padding: 16, backgroundColor: '#fafafa', borderRadius: 6 }}>
                  <Form form={form} onFinish={handleAddComment}>
                    <Form.Item
                      name="authorName"
                      rules={[{ required: true, message: 'Please enter your name' }]}
                    >
                      <Input placeholder="Your name" />
                    </Form.Item>
                    <Form.Item
                      name="content"
                      rules={[{ required: true, message: 'Please enter content' }]}
                    >
                      <TextArea rows={3} placeholder="Write a reply..." />
                    </Form.Item>
                    <Form.Item
                      name="position"
                    >
                      <Input placeholder="Position (optional)" />
                    </Form.Item>
                    <Form.Item>
                      <Space>
                        <Button
                          type="primary"
                          htmlType="submit"
                          loading={submitting}
                          icon={<SendOutlined />}
                        >
                          Reply
                        </Button>
                        <Button onClick={() => setReplyingToId(null)}>
                          Cancel
                        </Button>
                      </Space>
                    </Form.Item>
                  </Form>
                </div>
              )}

              {comment.replies && comment.replies.length > 0 && (
                <div style={{ marginTop: 16 }}>
                  <Divider style={{ margin: '8px 0' }} />
                  {comment.replies.map(reply => renderComment(reply, true))}
                </div>
              )}
            </div>
          }
        />
      </List.Item>
    </div>
  );

  return (
    <Card 
      title={`Comments (${comments.length})`}
      extra={
        <Button
          type="primary"
          icon={<MessageOutlined />}
          onClick={() => setReplyingToId(0)} // 0 indicates new top-level comment
        >
          Add Comment
        </Button>
      }
    >
      {replyingToId === 0 && (
        <Card size="small" style={{ marginBottom: 16 }}>
          <Form form={form} onFinish={handleAddComment}>
            <Form.Item
              name="authorName"
              rules={[{ required: true, message: 'Please enter your name' }]}
            >
              <Input placeholder="Your name" />
            </Form.Item>
            <Form.Item
              name="content"
              rules={[{ required: true, message: 'Please enter content' }]}
            >
              <TextArea rows={4} placeholder="Write a comment..." />
            </Form.Item>
            <Form.Item
              name="position"
            >
              <Input placeholder="Position (optional - e.g., page 1, line 5)" />
            </Form.Item>
            <Form.Item>
              <Space>
                <Button
                  type="primary"
                  htmlType="submit"
                  loading={submitting}
                  icon={<SendOutlined />}
                >
                  Add Comment
                </Button>
                <Button onClick={() => setReplyingToId(null)}>
                  Cancel
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Card>
      )}

      <List
        loading={loading}
        dataSource={comments.filter(comment => !comment.parentCommentId)}
        renderItem={comment => renderComment(comment)}
      />
    </Card>
  );
};

export default DocumentComments;
