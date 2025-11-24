import React, { useState } from 'react';
import { Form, Input, Button, Card, Typography, message, Space, Result, Alert } from 'antd';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { MailOutlined, SafetyOutlined, LoadingOutlined } from '@ant-design/icons';
import authService from '../services/authService';

const { Title, Text } = Typography;

const EmailVerification: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [verified, setVerified] = useState(false);
  const [loggingIn, setLoggingIn] = useState(false);
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
      console.log('🔍 Verifying email:', email);
      const response = await authService.verifyEmail(email, values.code);

      console.log('✅ Verification response:', response);

      if (response.success) {
        message.success(response.message || 'Email verified successfully!');
        setVerified(true);

        // Get stored credentials from sessionStorage (set during signup)
        const storedUsername = sessionStorage.getItem('signup_username');
        const storedPassword = sessionStorage.getItem('signup_password');

        console.log('📦 Stored credentials check:', {
          hasUsername: !!storedUsername,
          hasPassword: !!storedPassword,
          username: storedUsername
        });

        if (storedUsername && storedPassword) {
          // Auto-login after verification
          setLoggingIn(true);
          try {
            console.log('🔐 Attempting auto-login for user:', storedUsername);
            const loginResult = await authService.login(storedUsername, storedPassword);
            
            console.log('✅ Login successful:', loginResult);
            
            // Clear stored credentials
            sessionStorage.removeItem('signup_username');
            sessionStorage.removeItem('signup_password');
            
            message.success('You are now logged in. Redirecting to identity card upload...');
            
            // Redirect to identity card upload
            setTimeout(() => {
              console.log('🚀 Redirecting to /upload-identity-card');
              navigate('/upload-identity-card');
            }, 1500);
          } catch (loginError: any) {
            console.error('❌ Auto-login failed:', loginError);
            console.error('Error details:', {
              message: loginError.message,
              response: loginError.response?.data,
              status: loginError.response?.status
            });
            
            message.warning('Email verified! Please login manually to upload your identity card.');
            
            // Redirect to login if auto-login fails
            setTimeout(() => {
              console.log('🚀 Redirecting to login page');
              navigate(`/login?email=${email}&redirect=/upload-identity-card`);
            }, 2000);
          } finally {
            setLoggingIn(false);
          }
        } else {
          // No stored credentials, redirect to login
          console.warn('⚠️ No stored credentials found');
          message.info('Please login to upload your identity card.');
          setTimeout(() => {
            console.log('🚀 Redirecting to login page (no credentials)');
            navigate(`/login?email=${email}&redirect=/upload-identity-card`);
          }, 2000);
        }
      } else {
        message.error(response.message || 'Verification failed. Please try again.');
      }
    } catch (error: any) {
      console.error('❌ Verification error:', error);
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

  if (verified && loggingIn) {
    return (
      <div style={{ maxWidth: 500, margin: '50px auto', padding: '0 20px' }}>
        <Card>
          <Space direction="vertical" size="large" style={{ width: '100%', textAlign: 'center' }}>
            <LoadingOutlined style={{ fontSize: 48, color: '#52c41a' }} spin />
            <Title level={3}>Logging you in...</Title>
            <Text type="secondary">Please wait while we set up your account</Text>
          </Space>
        </Card>
      </div>
    );
  }

  if (verified) {
    return (
      <div style={{ maxWidth: 500, margin: '50px auto', padding: '0 20px' }}>
        <Result
          status="success"
          title="Email Verified Successfully!"
          subTitle="Redirecting you to upload your identity card..."
          extra={[
            <Button type="primary" key="upload" onClick={() => navigate('/upload-identity-card')}>
              Upload Identity Card Now
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

          <Alert
            message="After Verification"
            description="Once verified, you'll be automatically logged in and redirected to upload your identity card for admin verification."
            type="info"
            showIcon
          />

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