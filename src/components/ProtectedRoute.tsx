import React from 'react';
import { Navigate } from 'react-router-dom';
import { Result, Button, Spin } from 'antd';
import { LockOutlined } from '@ant-design/icons';
import { useAuth } from '../contexts/AuthContext';

interface ProtectedRouteProps {
  children: React.ReactNode;
  allowedRoles?: string[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, allowedRoles }) => {
  const { isAuthenticated, loading, hasRole } = useAuth();

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
        <Spin size="large" tip="Loading..." />
      </div>
    );
  }

  if (!isAuthenticated) {
    // Redirect to login if not authenticated
    return <Navigate to="/login" replace />;
  }

  // Check if user has required role
  if (allowedRoles && allowedRoles.length > 0) {
    const hasRequiredRole = allowedRoles.some(role => hasRole(role));

    if (!hasRequiredRole) {
      return (
        <div style={{ padding: '50px' }}>
          <Result
            status="403"
            icon={<LockOutlined style={{ color: '#ff4d4f' }} />}
            title="Access Denied"
            subTitle="Sorry, you don't have permission to access this page. This page is only available to Rescuers."
            extra={
              <Button type="primary" href="/rescue-requests">
                Go to Rescue Requests
              </Button>
            }
          />
        </div>
      );
    }
  }

  return <>{children}</>;
};

export default ProtectedRoute;
