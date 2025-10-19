import { useState } from 'react';
import { Card, Upload, Button, Form, Input, message, Progress, Typography } from 'antd';
import { UploadOutlined, InboxOutlined } from '@ant-design/icons';
import type { UploadFile } from 'antd/es/upload/interface';
import { useDocuments } from '../hooks/useDocuments';
import { useNavigate } from 'react-router-dom';

const { Dragger } = Upload;
const { Title } = Typography;

const UploadPage = () => {
  const [form] = Form.useForm();
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const { uploadFile, uploadProgress, loading } = useDocuments();
  const navigate = useNavigate();

  const handleUpload = async (values: any) => {
    if (fileList.length === 0) {
      message.error('Please select a file to upload');
      return;
    }

    const file = fileList[0].originFileObj as File;
    const success = await uploadFile(file, values.title);

    if (success) {
      message.success('Document uploaded successfully!');
      form.resetFields();
      setFileList([]);
      navigate('/documents');
    } else {
      message.error('Upload failed. Please try again.');
    }
  };

  const uploadProps = {
    fileList,
    beforeUpload: (file: File) => {
      // Validate file type
      const isValidType = [
        'application/pdf',
        'image/jpeg',
        'image/png',
        'text/plain',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
      ].includes(file.type);

      if (!isValidType) {
        message.error('Invalid file type. Please upload PDF, images, or documents.');
        return false;
      }

      // Validate file size (50MB)
      const isLt50M = file.size / 1024 / 1024 < 50;
      if (!isLt50M) {
        message.error('File must be smaller than 50MB!');
        return false;
      }

      setFileList([{
        uid: file.name + Date.now(),
        name: file.name,
        status: 'done',
        originFileObj: file as any,
      }]);

      return false; // Prevent auto upload
    },
    onRemove: () => {
      setFileList([]);
    },
  };

  return (
    <div>
      <Title level={2}>Upload Document</Title>
      
      <Card>
        <Form form={form} layout="vertical" onFinish={handleUpload}>
          <Form.Item name="title" label="Document Title">
            <Input placeholder="Enter document title (optional)" />
          </Form.Item>

          <Form.Item label="Select File" required>
            <Dragger {...uploadProps} style={{ padding: '40px' }}>
              <p className="ant-upload-drag-icon">
                <InboxOutlined style={{ fontSize: 48, color: '#1890ff' }} />
              </p>
              <p className="ant-upload-text">
                Click or drag file to this area to upload
              </p>
              <p className="ant-upload-hint">
                Support PDF, Word documents, images, and text files. Maximum size: 50MB
              </p>
            </Dragger>
          </Form.Item>

          {loading && uploadProgress > 0 && (
            <Form.Item>
              <Progress percent={uploadProgress} status="active" />
            </Form.Item>
          )}

          <Form.Item>
            <Button 
              type="primary" 
              htmlType="submit" 
              loading={loading}
              disabled={fileList.length === 0}
              icon={<UploadOutlined />}
            >
              Upload Document
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};

export default UploadPage;
