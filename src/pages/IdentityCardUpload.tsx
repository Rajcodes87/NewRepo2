import React, { useState, useEffect } from 'react';
import { Form, Button, Card, Typography, message, Upload, Space, Result, Alert, Spin } from 'antd';
import { useNavigate } from 'react-router-dom';
import { UploadOutlined, IdcardOutlined, CheckCircleOutlined, LoadingOutlined } from '@ant-design/icons';
import type { UploadFile, RcFile } from 'antd/es/upload/interface';
import rescuerApplicationService from '../services/rescuerApplicationService';
import authService from '../services/authService';

const { Title, Text, Paragraph } = Typography;

const IdentityCardUpload: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [base64Image, setBase64Image] = useState<string>('');
  const [checking, setChecking] = useState(true);
  const navigate = useNavigate();

  // Check authentication with a delay to allow token to be set
  useEffect(() => {
    const checkAuth = async () => {
      console.log('🔐 IdentityCardUpload - Checking authentication...');
      
      // Wait a bit for token to be set in localStorage
      await new Promise(resolve => setTimeout(resolve, 500));
      
      const isAuth = authService.isAuthenticated();
      console.log('🔐 IdentityCardUpload - Is authenticated:', isAuth);
      
      if (!isAuth) {
        console.warn('⚠️ IdentityCardUpload - Not authenticated, redirecting to login');
        message.warning('Please login to continue');
        navigate('/login?redirect=/upload-identity-card');
      } else {
        console.log('✅ IdentityCardUpload - Authenticated, showing form');
      }
      
      setChecking(false);
    };

    checkAuth();
  }, [navigate]);

  // Convert file to base64
  const getBase64 = (file: RcFile): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = (error) => reject(error);
    });
  };

  // Handle file upload change
  const handleUploadChange = async (info: any) => {
    let newFileList = [...info.fileList];

    // Limit to 1 file
    newFileList = newFileList.slice(-1);

    setFileList(newFileList);

    if (newFileList.length > 0 && newFileList[0].originFileObj) {
      try {
        const base64 = await getBase64(newFileList[0].originFileObj as RcFile);
        setBase64Image(base64);
      } catch (error) {
        message.error('Error reading file');
      }
    } else {
      setBase64Image('');
    }
  };

  // Validate file before upload
  const beforeUpload = (file: RcFile) => {
    const isImage = file.type.startsWith('image/');
    if (!isImage) {
      message.error('You can only upload image files!');
      return Upload.LIST_IGNORE;
    }

    const isLt5M = file.size / 1024 / 1024 < 5;
    if (!isLt5M) {
      message.error('Image must be smaller than 5MB!');
      return Upload.LIST_IGNORE;
    }

    return false; // Prevent auto upload
  };

  const handleSubmit = async () => {
    if (!base64Image) {
      message.error('Please upload your identity card image');
      return;
    }

    if (!authService.isAuthenticated()) {
      message.error('You must be logged in to submit your identity card');
      navigate('/login?redirect=/upload-identity-card');
      return;
    }

    setLoading(true);
    try {
      const response = await rescuerApplicationService.submitApplication({
        identityCardPicture: base64Image,
      });

      if (response.success) {
        message.success(response.message || 'Identity card submitted successfully!');
        setSubmitted(true);
      } else {
        message.error(response.message || 'Failed to submit identity card');
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.error?.message ||
        'An error occurred while submitting your identity card. Please try again.';
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  // Show loading while checking authentication
  if (checking) {
    return (
      <div style={{ maxWidth: 600, margin: '50px auto', padding: '0 20px', textAlign: 'center' }}>
        <Card>
          <Space direction="vertical" size="large" style={{ width: '100%' }}>
            <LoadingOutlined style={{ fontSize: 48, color: '#1890ff' }} spin />
            <Title level={3}>Loading...</Title>
            <Text type="secondary">Please wait while we verify your authentication</Text>
          </Space>
        </Card>
      </div>
    );
  }

  if (submitted) {
    return (
      <div style={{ maxWidth: 600, margin: '50px auto', padding: '0 20px' }}>
        <Result
          status="success"
          icon={<CheckCircleOutlined style={{ color: '#52c41a' }} />}
          title="Identity Card Submitted Successfully!"
          subTitle="Your identity card has been submitted for admin verification. You will receive an email notification once your application is reviewed. This usually takes 24-48 hours."
          extra={[
            <Button type="primary" key="home" onClick={() => navigate('/rescue-requests')}>
              Go to Home
            </Button>,
            <Button key="logout" onClick={() => {
              authService.logout();
              navigate('/login');
            }}>
              Logout
            </Button>,
          ]}
        />
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 600, margin: '50px auto', padding: '0 20px' }}>
      <Card>
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div style={{ textAlign: 'center' }}>
            <IdcardOutlined style={{ fontSize: 48, color: '#1890ff' }} />
            <Title level={2}>Complete Your Verification</Title>
            <Text type="secondary">
              To become a verified rescuer, please upload a clear photo of your identity card (ID, Passport, or Driver's License)
            </Text>
          </div>

          <Alert
            message="Important Information"
            description={
              <ul style={{ marginBottom: 0, paddingLeft: 20 }}>
                <li>Ensure the photo is clear and all details are readable</li>
                <li>Accepted formats: JPG, JPEG, PNG</li>
                <li>Maximum file size: 5MB</li>
                <li>Your information will be kept confidential</li>
                <li>Admin will review and verify within 24-48 hours</li>
              </ul>
            }
            type="info"
            showIcon
          />

          <Form form={form} layout="vertical">
            <Form.Item
              label="Identity Card Photo"
              name="identityCard"
              rules={[{ required: true, message: 'Please upload your identity card photo' }]}
            >
              <Upload
                listType="picture-card"
                fileList={fileList}
                onChange={handleUploadChange}
                beforeUpload={beforeUpload}
                accept="image/*"
                maxCount={1}
              >
                {fileList.length === 0 && (
                  <div>
                    <UploadOutlined />
                    <div style={{ marginTop: 8 }}>Upload</div>
                  </div>
                )}
              </Upload>
            </Form.Item>

            {base64Image && (
              <div style={{ marginTop: 16, textAlign: 'center' }}>
                <img
                  src={base64Image}
                  alt="Identity Card Preview"
                  style={{ maxWidth: '100%', maxHeight: 300, border: '1px solid #d9d9d9', borderRadius: 4 }}
                />
              </div>
            )}

            <Form.Item>
              <Button
                type="primary"
                onClick={handleSubmit}
                loading={loading}
                block
                size="large"
                disabled={!base64Image}
              >
                Submit for Verification
              </Button>
            </Form.Item>
          </Form>

          <div style={{ textAlign: 'center' }}>
            <Paragraph type="secondary" style={{ fontSize: 12 }}>
              By submitting this information, you agree to our terms and conditions and consent to identity verification.
            </Paragraph>
          </div>
        </Space>
      </Card>
    </div>
  );
};

export default IdentityCardUpload;