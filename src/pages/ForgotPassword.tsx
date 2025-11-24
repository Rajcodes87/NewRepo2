import React, { useState } from 'react';
import { Form, Input, Button, Card, Typography, message, Space } from 'antd';
import { useNavigate } from 'react-router-dom';
import { MailOutlined, LockOutlined } from '@ant-design/icons';
import authService from '../services/authService';

const { Title, Text } = Typography;

const ForgotPassword: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [emailSent, setEmailSent] = useState(false);
  const [submittedEmail, setSubmittedEmail] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (values: { email: string }) => {
    setLoading(true);
    try {
      const response = await authService.forgotPassword(values.email);

      if (response.success) {
        message.success(response.message || 'Password reset code sent to your email!');
        setEmailSent(true);
        setSubmittedEmail(values.email);

        // Redirect to reset password page after 2 seconds
        setTimeout(() => {
          navigate(`/reset-password?email=${encodeURIComponent(values.email)}`);
        }, 2000);
      } else {
        message.error(response.message || 'Failed to send reset code. Please try again.');
      }
    } catch (error: any) {
      const errorMessage = error.response?.data?.error?.message ||
                          'An error occurred. Please try again.';
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: 500, margin: '50px auto', padding: '0 20px' }}>
      <Card>
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div style={{ textAlign: 'center' }}>
            <LockOutlined style={{ fontSize: 48, color: '#1890ff' }} />
            <Title level={2}>Forgot Password</Title>
            <Text type="secondary">
              {emailSent
                ? 'Check your email for the reset code!'
                : 'Enter your email address and we\'ll send you a password reset code.'}
            </Text>
          </div>

          {!emailSent ? (
            <Form
              form={form}
              layout="vertical"
              onFinish={handleSubmit}
              autoComplete="off"
            >
              <Form.Item
                name="email"
                label="Email Address"
                rules={[
                  { required: true, message: 'Please enter your email' },
                  { type: 'email', message: 'Please enter a valid email address' },
                ]}
              >
                <Input
                  prefix={<MailOutlined />}
                  placeholder="Enter your email"
                  size="large"
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
                  Send Reset Code
                </Button>
              </Form.Item>
            </Form>
          ) : (
            <div style={{ textAlign: 'center' }}>
              <Text>
                A password reset code has been sent to <Text strong>{submittedEmail}</Text>
              </Text>
              <div style={{ marginTop: 20 }}>
                <Button
                  type="primary"
                  size="large"
                  onClick={() => navigate(`/reset-password?email=${encodeURIComponent(submittedEmail)}`)}
                >
                  Continue to Reset Password
                </Button>
              </div>
            </div>
          )}

          <div style={{ textAlign: 'center', marginTop: 16 }}>
            <Text type="secondary">Remember your password? </Text>
            <Button type="link" onClick={() => navigate('/login')} style={{ padding: 0 }}>
              Back to Login
            </Button>
          </div>
        </Space>
      </Card>
    </div>
  );
};

export default ForgotPassword;
