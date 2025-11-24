import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link, Navigate, useNavigate } from 'react-router-dom';
import { Layout, Menu, Typography, Button, Dropdown, Space, Avatar } from 'antd';
import {
  HomeOutlined,
  PhoneOutlined,
  RocketOutlined,
  CheckCircleOutlined,
  LoginOutlined,
  LogoutOutlined,
  UserOutlined,
  TeamOutlined,
} from '@ant-design/icons';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import RequestRescueList from './pages/RequestRescueList';
import RescueInitiationList from './pages/RescueInitiationList';
import RescueCompletionList from './pages/RescueCompletionList';
import Login from './pages/Login';
import SignupRescuer from './pages/SignupRescuer';
import EmailVerification from './pages/EmailVerification';
import ForgotPassword from './pages/ForgotPassword';
import ResetPassword from './pages/ResetPassword';
import IdentityCardUpload from './pages/IdentityCardUpload';
import RescuerApplicationList from './pages/RescuerApplicationList';
import ShareLocation from './pages/ShareLocation';
import NearestRescuersView from './pages/NearestRescuersView';
import './App.css';

const { Header, Content, Footer } = Layout;
const { Title } = Typography;

const AppContent: React.FC = () => {
  const [selectedKey, setSelectedKey] = React.useState('rescue-requests');
  const navigate = useNavigate();
  const { isAuthenticated, user, logout, hasRole } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  // Build menu items based on authentication and role
  const menuItems = [
    {
      key: 'rescue-requests',
      icon: <PhoneOutlined />,
      label: <Link to="/rescue-requests">Rescue Requests</Link>,
    },
  ];

  // Only show these menu items if user is authenticated and has Rescuer or admin role
  if (isAuthenticated && (hasRole('Rescuer') || hasRole('admin'))) {
    menuItems.push(
      {
        key: 'rescue-initiations',
        icon: <RocketOutlined />,
        label: <Link to="/rescue-initiations">Rescue Initiations</Link>,
      },
      {
        key: 'rescue-completions',
        icon: <CheckCircleOutlined />,
        label: <Link to="/rescue-completions">Rescue Completions</Link>,
      }
    );
  }

  // Admin-only menu items
  if (isAuthenticated && hasRole('admin')) {
    menuItems.push({
      key: 'rescuer-applications',
      icon: <TeamOutlined />,
      label: <Link to="/admin/rescuer-applications">Rescuer Applications</Link>,
    });
  }

  // User dropdown menu
  const userMenuItems = [
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Logout',
      onClick: handleLogout,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center', gap: '24px', paddingInline: '24px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <HomeOutlined style={{ fontSize: '24px', color: 'white' }} />
          <Title level={3} style={{ color: 'white', margin: 0 }}>
            Pawchums
          </Title>
        </div>
        <Menu
          theme="dark"
          mode="horizontal"
          selectedKeys={[selectedKey]}
          items={menuItems}
          style={{ flex: 1, minWidth: 0 }} // ✅ FIXED: Added missing colon
          onSelect={({ key }) => setSelectedKey(key)}
        />
        <div style={{ marginLeft: 'auto' }}>
          {isAuthenticated ? (
            <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
              <Space style={{ cursor: 'pointer', color: 'white' }}>
                <Avatar icon={<UserOutlined />} style={{ backgroundColor: '#52c41a' }} />
                <span>{user?.name || user?.email || 'User'}</span>
                {(hasRole('admin') && (
                  <span style={{ fontSize: '12px', color: '#faad14' }}>[Admin]</span>
                )) || (hasRole('Rescuer') && (
                  <span style={{ fontSize: '12px', color: '#52c41a' }}>[Rescuer]</span>
                ))}
              </Space>
            </Dropdown>
          ) : (
            <Button
              type="primary"
              icon={<LoginOutlined />}
              onClick={() => navigate('/login')}
              style={{ backgroundColor: '#52c41a', borderColor: '#52c41a' }}
            >
              Login
            </Button>
          )}
        </div>
      </Header>
      <Content style={{ minHeight: 'calc(100vh - 134px)' }}>
        <Routes>
          <Route path="/" element={<Navigate to="/rescue-requests" replace />} />
          <Route path="/login" element={<Login />} />
          <Route path="/signup-rescuer" element={<SignupRescuer />} />
          <Route path="/verify-email" element={<EmailVerification />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/reset-password" element={<ResetPassword />} />
          <Route path="/upload-identity-card" element={<IdentityCardUpload />} />
          <Route path="/share-location" element={<ShareLocation />} />
          {/* Public route */}
          <Route path="/rescue-requests" element={<RequestRescueList />} />
          <Route 
  path="/rescue-requests/:requestId/nearest-rescuers" 
  element={
    <ProtectedRoute allowedRoles={['admin']}>
      <NearestRescuersView />
    </ProtectedRoute>
  } 
/>
          {/* Protected routes - only for Rescuers and admins */}
          <Route
            path="/rescue-initiations"
            element={
              <ProtectedRoute allowedRoles={['Rescuer', 'admin']}>
                <RescueInitiationList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/rescue-completions"
            element={
              <ProtectedRoute allowedRoles={['Rescuer', 'admin']}>
                <RescueCompletionList />
              </ProtectedRoute>
            }
          />

          {/* Admin-only routes */}
          <Route
            path="/admin/rescuer-applications"
            element={
              <ProtectedRoute allowedRoles={['admin']}>
                <RescuerApplicationList />
              </ProtectedRoute>
            }
          />
        </Routes>
      </Content>
      <Footer style={{ textAlign: 'center' }}>
        Pawchums Animal Rescue System ©{new Date().getFullYear()} - Saving Lives, One Paw at a Time
      </Footer>
    </Layout>
  );
};

const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <AppContent />
      </Router>
    </AuthProvider>
  );
};

export default App;