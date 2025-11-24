import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Form, Input, Button, Card, Typography, message, Row, Col, Divider } from 'antd';
import { UserOutlined, LockOutlined, CrownOutlined, HeartOutlined } from '@ant-design/icons';
import { useAuth } from '../contexts/AuthContext';

const { Title, Text } = Typography;

interface LoginFormValues {
  username: string;
  password: string;
}

const Login: React.FC = () => {
  const [adminLoading, setAdminLoading] = useState(false);
  const [rescuerLoading, setRescuerLoading] = useState(false);
  const [adminForm] = Form.useForm();
  const [rescuerForm] = Form.useForm();
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleAdminLogin = async (values: LoginFormValues) => {
    setAdminLoading(true);
    try {
      await login(values.username, values.password);
      message.success('Admin login successful!');
      navigate('/rescue-requests');
    } catch (error: any) {
      message.error(error.message || 'Login failed. Please check your credentials.');
    } finally {
      setAdminLoading(false);
    }
  };

  const handleRescuerLogin = async (values: LoginFormValues) => {
    setRescuerLoading(true);
    try {
      await login(values.username, values.password);
      message.success('Welcome, Rescuer!');
      navigate('/rescue-initiations');
    } catch (error: any) {
      message.error(error.message || 'Login failed. Please check your credentials.');
    } finally {
      setRescuerLoading(false);
    }
  };

  return (
    <div style={{ minHeight: '100vh', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', padding: '50px 20px' }}>
      <div style={{ maxWidth: '1200px', margin: '0 auto' }}>
        <Title level={1} style={{ textAlign: 'center', color: 'white', marginBottom: 50 }}>
          Pawchums Animal Rescue System
        </Title>

        <Row gutter={[32, 32]}>
          {/* Admin Login */}
          <Col xs={24} lg={12}>
            <Card
              hoverable
              style={{ height: '100%' }}
              styles={{
                body: { padding: '40px' }
              }}
            >
              <div style={{ textAlign: 'center', marginBottom: 30 }}>
                <CrownOutlined style={{ fontSize: 48, color: '#faad14' }} />
                <Title level={2} style={{ marginTop: 16, marginBottom: 8 }}>
                  Admin Login
                </Title>
                <Text type="secondary">
                  For system administrators only
                </Text>
              </div>

              <Form
                form={adminForm}
                name="admin-login"
                onFinish={handleAdminLogin}
                autoComplete="off"
                layout="vertical"
                size="large"
              >
                <Form.Item
                  name="username"
                  rules={[{ required: true, message: 'Please enter your username!' }]}
                >
                  <Input
                    prefix={<UserOutlined />}
                    placeholder="Username"
                  />
                </Form.Item>

                <Form.Item
                  name="password"
                  rules={[{ required: true, message: 'Please enter your password!' }]}
                >
                  <Input.Password
                    prefix={<LockOutlined />}
                    placeholder="Password"
                  />
                </Form.Item>

                <Form.Item>
                  <Button
                    type="primary"
                    htmlType="submit"
                    loading={adminLoading}
                    block
                    size="large"
                    style={{ backgroundColor: '#faad14', borderColor: '#faad14' }}
                  >
                    Login as Admin
                  </Button>
                </Form.Item>
              </Form>
            </Card>
          </Col>

          {/* Rescuer Login */}
          <Col xs={24} lg={12}>
            <Card
              hoverable
              style={{ height: '100%' }}
              styles={{
                body: { padding: '40px' }
              }}
            >
              <div style={{ textAlign: 'center', marginBottom: 30 }}>
                <HeartOutlined style={{ fontSize: 48, color: '#52c41a' }} />
                <Title level={2} style={{ marginTop: 16, marginBottom: 8 }}>
                  Rescuer Login
                </Title>
                <Text type="secondary">
                  Join our mission to save animals
                </Text>
              </div>

              <Form
                form={rescuerForm}
                name="rescuer-login"
                onFinish={handleRescuerLogin}
                autoComplete="off"
                layout="vertical"
                size="large"
              >
                <Form.Item
                  name="username"
                  rules={[{ required: true, message: 'Please enter your username!' }]}
                >
                  <Input
                    prefix={<UserOutlined />}
                    placeholder="Username"
                  />
                </Form.Item>

                <Form.Item
                  name="password"
                  rules={[{ required: true, message: 'Please enter your password!' }]}
                >
                  <Input.Password
                    prefix={<LockOutlined />}
                    placeholder="Password"
                  />
                </Form.Item>

                <Form.Item>
                  <div style={{ textAlign: 'right', marginBottom: 16 }}>
                    <Link to="/forgot-password" style={{ color: '#52c41a' }}>
                      Forgot Password?
                    </Link>
                  </div>
                  <Button
                    type="primary"
                    htmlType="submit"
                    loading={rescuerLoading}
                    block
                    size="large"
                    style={{ backgroundColor: '#52c41a', borderColor: '#52c41a' }}
                  >
                    Login as Rescuer
                  </Button>
                </Form.Item>
              </Form>

              <Divider>New Rescuer?</Divider>

              <Button
                type="default"
                block
                size="large"
                onClick={() => navigate('/signup-rescuer')}
              >
                Sign Up as Rescuer
              </Button>
            </Card>
          </Col>
        </Row>

        <div style={{ textAlign: 'center', marginTop: 40 }}>
          <Button
            type="link"
            size="large"
            onClick={() => navigate('/rescue-requests')}
            style={{ color: 'white' }}
          >
            Continue as Guest (View Rescue Requests)
          </Button>
        </div>
      </div>
    </div>
  );
};

export default Login;
