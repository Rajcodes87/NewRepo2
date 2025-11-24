import React, { useState } from 'react';
import { Form, Input, Button, Card, Typography, message, Space, Result } from 'antd';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { LockOutlined, SafetyOutlined } from '@ant-design/icons';
import authService from '../services/authService';

const { Title, Text } = Typography;

interface ResetPasswordFormValues {
  code: string;
  newPassword: string;
  confirmPassword: string;
}

const ResetPassword: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [resetSuccess, setResetSuccess] = useState(false);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email') || '';

  const handleSubmit = async (values: ResetPasswordFormValues) => {
    if (!email) {
      message.error('Email is required. Please go back to forgot password.');
      return;
    }

    setLoading(true);
    try {
      const response = await authService.resetPassword(
        email,
        values.code,
        values.newPassword
      );

      if (response.success) {
        message.success(response.message || 'Password reset successfully!');
        setResetSuccess(true);

        // Redirect to login after 2 seconds
        setTimeout(() => {
          navigate('/login');
        }, 2000);
      } else {
        message.error(response.message || 'Password reset failed. Please try again.');
      }
    } catch (error: any) {
      const errorMessage = error.response?.data?.error?.message ||
                          'An error occurred during password reset. Please try again.';
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  if (resetSuccess) {
    return (
      <div style={{ maxWidth: 500, margin: '50px auto', padding: '0 20px' }}>
        <Result
          status="success"
          title="Password Reset Successfully!"
          subTitle="You can now log in with your new password. Redirecting to login page..."
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
            <LockOutlined style={{ fontSize: 48, color: '#1890ff' }} />
            <Title level={2}>Reset Password</Title>
            <Text type="secondary">
              Enter the verification code sent to:
            </Text>
            <div style={{ marginTop: 8 }}>
              <Text strong>{email}</Text>
            </div>
          </div>

          <Form
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
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

            <Form.Item
              name="newPassword"
              label="New Password"
              rules={[
                { required: true, message: 'Please enter your new password' },
                { min: 6, message: 'Password must be at least 6 characters' },
                { max: 128, message: 'Password must not exceed 128 characters' },
              ]}
              hasFeedback
            >
              <Input.Password
                prefix={<LockOutlined />}
                placeholder="Enter new password"
                size="large"
              />
            </Form.Item>

            <Form.Item
              name="confirmPassword"
              label="Confirm New Password"
              dependencies={['newPassword']}
              hasFeedback
              rules={[
                { required: true, message: 'Please confirm your new password' },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue('newPassword') === value) {
                      return Promise.resolve();
                    }
                    return Promise.reject(new Error('The two passwords do not match!'));
                  },
                }),
              ]}
            >
              <Input.Password
                prefix={<LockOutlined />}
                placeholder="Confirm new password"
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
                Reset Password
              </Button>
            </Form.Item>
          </Form>

          <div style={{ textAlign: 'center' }}>
            <Text type="secondary">Didn't receive the code? </Text>
            <Button
              type="link"
              onClick={() => navigate('/forgot-password')}
              style={{ padding: 0 }}
            >
              Resend Code
            </Button>
          </div>

          <div style={{ textAlign: 'center' }}>
            <Button type="link" onClick={() => navigate('/login')}>
              Back to Login
            </Button>
          </div>
        </Space>
      </Card>
    </div>
  );
};

export default ResetPassword;
