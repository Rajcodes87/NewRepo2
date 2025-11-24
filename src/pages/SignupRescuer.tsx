import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Form, Input, Button, Card, Typography, message, Row, Col } from 'antd';
import { UserOutlined, LockOutlined, MailOutlined, PhoneOutlined, HeartOutlined } from '@ant-design/icons';
import authService, { RegisterRescuerRequest } from '../services/authService';

const { Title, Text } = Typography;

interface SignupFormValues {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
  name?: string;
  surname?: string;
  phoneNumber?: string;
}

const SignupRescuer: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [form] = Form.useForm();
  const navigate = useNavigate();

  const handleSignup = async (values: SignupFormValues) => {
    setLoading(true);
    try {
      const request: RegisterRescuerRequest = {
        userName: values.userName,
        email: values.email,
        password: values.password,
        name: values.name,
        surname: values.surname,
        phoneNumber: values.phoneNumber,
      };

      const response = await authService.registerRescuer(request);

      if (response.success) {
        message.success(response.message || 'Registration successful! Please verify your email.');
        
        // ✅ Store credentials in sessionStorage for auto-login after email verification
        sessionStorage.setItem('signup_username', values.userName);
        sessionStorage.setItem('signup_password', values.password);
        
        // Redirect to email verification page with email parameter
        navigate(`/verify-email?email=${encodeURIComponent(values.email)}`);
      } else {
        message.error(response.message || 'Registration failed. Please try again.');
      }
    } catch (error: any) {
      const errorMessage = error.response?.data?.error?.message ||
                          error.message ||
                          'Registration failed. Please try again.';
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ minHeight: '100vh', background: 'linear-gradient(135deg, #52c41a 0%, #73d13d 100%)', padding: '50px 20px' }}>
      <div style={{ maxWidth: '600px', margin: '0 auto' }}>
        <Title level={1} style={{ textAlign: 'center', color: 'white', marginBottom: 20 }}>
          Become a Rescuer
        </Title>
        <Text style={{ display: 'block', textAlign: 'center', color: 'white', fontSize: 16, marginBottom: 40 }}>
          Join our community and help save animals in need
        </Text>

        <Card
          styles={{
            body: { padding: '40px' }
          }}
        >
          <div style={{ textAlign: 'center', marginBottom: 30 }}>
            <HeartOutlined style={{ fontSize: 48, color: '#52c41a' }} />
            <Title level={2} style={{ marginTop: 16, marginBottom: 8 }}>
              Rescuer Registration
            </Title>
          </div>

          <Form
            form={form}
            name="rescuer-signup"
            onFinish={handleSignup}
            autoComplete="off"
            layout="vertical"
            size="large"
          >
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  label="First Name"
                  name="name"
                >
                  <Input placeholder="John" />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  label="Last Name"
                  name="surname"
                >
                  <Input placeholder="Doe" />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              label="Username"
              name="userName"
              rules={[
                { required: true, message: 'Please enter a username!' },
                { min: 3, message: 'Username must be at least 3 characters!' },
                { max: 256, message: 'Username must not exceed 256 characters!' },
              ]}
            >
              <Input prefix={<UserOutlined />} placeholder="johndoe" />
            </Form.Item>

            <Form.Item
              label="Email"
              name="email"
              rules={[
                { required: true, message: 'Please enter your email!' },
                { type: 'email', message: 'Please enter a valid email!' },
              ]}
            >
              <Input prefix={<MailOutlined />} placeholder="john@example.com" />
            </Form.Item>

            <Form.Item
              label="Phone Number"
              name="phoneNumber"
            >
              <Input prefix={<PhoneOutlined />} placeholder="+1234567890" />
            </Form.Item>

            <Form.Item
              label="Password"
              name="password"
              rules={[
                { required: true, message: 'Please enter a password!' },
                { min: 6, message: 'Password must be at least 6 characters!' },
                {
                  pattern: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]/,
                  message: 'Password must contain uppercase, lowercase, number, and special character!'
                },
              ]}
              hasFeedback
            >
              <Input.Password prefix={<LockOutlined />} placeholder="••••••••" />
            </Form.Item>

            <Form.Item
              label="Confirm Password"
              name="confirmPassword"
              dependencies={['password']}
              hasFeedback
              rules={[
                { required: true, message: 'Please confirm your password!' },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue('password') === value) {
                      return Promise.resolve();
                    }
                    return Promise.reject(new Error('Passwords do not match!'));
                  },
                }),
              ]}
            >
              <Input.Password prefix={<LockOutlined />} placeholder="••••••••" />
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                loading={loading}
                block
                size="large"
                style={{ backgroundColor: '#52c41a', borderColor: '#52c41a', marginTop: 16 }}
              >
                Sign Up as Rescuer
              </Button>
            </Form.Item>
          </Form>

          <div style={{ textAlign: 'center', marginTop: 16 }}>
            <Text>
              Already have an account?{' '}
              <Link to="/login" style={{ color: '#52c41a', fontWeight: 'bold' }}>
                Login here
              </Link>
            </Text>
          </div>
        </Card>

        <div style={{ textAlign: 'center', marginTop: 30 }}>
          <Button
            type="link"
            size="large"
            onClick={() => navigate('/rescue-requests')}
            style={{ color: 'white' }}
          >
            ← Back to Rescue Requests
          </Button>
        </div>
      </div>
    </div>
  );
};

export default SignupRescuer;