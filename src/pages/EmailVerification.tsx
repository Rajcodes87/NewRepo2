import React, { useState } from 'react';
import { Form, Input, Button, Card, Typography, message, Space, Result } from 'antd';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { MailOutlined, SafetyOutlined } from '@ant-design/icons';
import authService from '../services/authService';

const { Title, Text } = Typography;

const EmailVerification: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [verified, setVerified] = useState(false);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email') || '';

  const handleVerify = async (values: { code: string }) => {
    if (!email) {
      message.error('Email is required. Please go back to registration.');
      return;
    }

    setLoading(true);
    try {
      const response = await authService.verifyEmail(email, values.code);

      if (response.success) {
        message.success(response.message || 'Email verified successfully!');
        setVerified(true);

        // Redirect to login after 2 seconds
        setTimeout(() => {
          navigate('/login');
        }, 2000);
      } else {
        message.error(response.message || 'Verification failed. Please try again.');
      }
    } catch (error: any) {
      const errorMessage = error.response?.data?.error?.message ||
                          'An error occurred during verification. Please try again.';
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleResendCode = async () => {
    message.info('Resend functionality will be implemented soon.');
    // TODO: Implement resend verification code API
  };

  if (verified) {
    return (
      <div style={{ maxWidth: 500, margin: '50px auto', padding: '0 20px' }}>
        <Result
          status="success"
          title="Email Verified Successfully!"
          subTitle="Your account is now active. You will be redirected to login page..."
          extra={[
            <Button type="primary" key="login" onClick={() => navigate('/login')}>
              Go to Login
            </Button>,
          ]}
        />
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 500, margin: '50px auto', padding: '0 20px' }}>
      <Card>
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div style={{ textAlign: 'center' }}>
            <MailOutlined style={{ fontSize: 48, color: '#1890ff' }} />
            <Title level={2}>Verify Your Email</Title>
            <Text type="secondary">
              We've sent a 6-digit verification code to:
            </Text>
            <div style={{ marginTop: 8 }}>
              <Text strong>{email}</Text>
            </div>
          </div>

          <Form
            form={form}
            layout="vertical"
            onFinish={handleVerify}
            autoComplete="off"
          >
            <Form.Item
              name="code"
              label="Verification Code"
              rules={[
                { required: true, message: 'Please enter the verification code' },
                { len: 6, message: 'Verification code must be 6 digits' },
                { pattern: /^\d+$/, message: 'Verification code must contain only numbers' },
              ]}
            >
              <Input
                prefix={<SafetyOutlined />}
                placeholder="Enter 6-digit code"
                maxLength={6}
                size="large"
                style={{ textAlign: 'center', fontSize: 20, letterSpacing: 5 }}
              />
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                loading={loading}
                block
                size="large"
              >
                Verify Email
              </Button>
            </Form.Item>
          </Form>

          <div style={{ textAlign: 'center' }}>
            <Text type="secondary">Didn't receive the code? </Text>
            <Button type="link" onClick={handleResendCode} style={{ padding: 0 }}>
              Resend Code
            </Button>
          </div>

          <div style={{ textAlign: 'center' }}>
            <Button type="link" onClick={() => navigate('/signup-rescuer')}>
              Back to Signup
            </Button>
          </div>
        </Space>
      </Card>
    </div>
  );
};

export default EmailVerification;
