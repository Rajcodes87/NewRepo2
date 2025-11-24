import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Card, Typography, Button, message, Result, Spin, Alert, Space } from 'antd';
import { EnvironmentOutlined, CheckCircleOutlined, LoadingOutlined } from '@ant-design/icons';
import LocationPicker from '../components/LocationPicker';
import RescuerProfileService from '../services/rescuerProfileService';
import authService from '../services/authService';

const { Title, Text, Paragraph } = Typography;

const ShareLocation: React.FC = () => {
  const [searchParams] = useSearchParams();
  const [loading, setLoading] = useState(false);
  const [validating, setValidating] = useState(true);
  const [submitted, setSubmitted] = useState(false);
  const [tokenValid, setTokenValid] = useState(false);
  const [distance, setDistance] = useState<number | null>(null);
  const navigate = useNavigate();

  const token = searchParams.get('token');
  const requestId = searchParams.get('requestId');

  useEffect(() => {
    // Validate token and check authentication
    const validateAccess = async () => {
      if (!token || !requestId) {
        message.error('Invalid link. Missing token or request ID.');
        setValidating(false);
        return;
      }

      // Check if user is authenticated
      const isAuth = authService.isAuthenticated();
      if (!isAuth) {
        message.warning('Please login to share your location');
        setTimeout(() => {
          navigate(`/login?redirect=${encodeURIComponent(window.location.pathname + window.location.search)}`);
        }, 2000);
        return;
      }

      setTokenValid(true);
      setValidating(false);
    };

    validateAccess();
  }, [token, requestId, navigate]);

  const handleLocationShare = async (lat: number, lng: number, address?: string) => {
    if (!token || !requestId) {
      message.error('Invalid request parameters');
      return;
    }

    setLoading(true);
    try {
      const response = await RescuerProfileService.shareLocation({
        token,
        requestRescueId: requestId,
        latitude: lat,
        longitude: lng,
        currentLocation: address,
      });

      message.success('Location shared successfully!');
      
      // Extract distance from response
      if (response && typeof response === 'object' && 'distance' in response) {
        setDistance(response.distance);
      }
      
      setSubmitted(true);
    } catch (error: any) {
      console.error('Error sharing location:', error);
      const errorMessage =
        error.response?.data?.error?.message || 
        error.message ||
        'An error occurred while sharing your location';
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  if (validating) {
    return (
      <div style={{ maxWidth: 800, margin: '50px auto', padding: '0 20px', textAlign: 'center' }}>
        <Card>
          <Space direction="vertical" size="large" style={{ width: '100%' }}>
            <Spin indicator={<LoadingOutlined style={{ fontSize: 48 }} spin />} />
            <Title level={3}>Validating your request...</Title>
            <Text type="secondary">Please wait while we verify your access</Text>
          </Space>
        </Card>
      </div>
    );
  }

  if (!tokenValid) {
    return (
      <div style={{ maxWidth: 800, margin: '50px auto', padding: '0 20px' }}>
        <Result
          status="error"
          title="Invalid or Expired Link"
          subTitle="This location sharing link is either invalid or has expired. Please check your email for a new link or contact support."
          extra={[
            <Button type="primary" key="requests" onClick={() => navigate('/rescue-requests')}>
              View Rescue Requests
            </Button>,
            <Button key="login" onClick={() => navigate('/login')}>
              Login
            </Button>,
          ]}
        />
      </div>
    );
  }

  if (submitted) {
    return (
      <div style={{ maxWidth: 800, margin: '50px auto', padding: '0 20px' }}>
        <Result
          status="success"
          icon={<CheckCircleOutlined style={{ color: '#52c41a' }} />}
          title="Location Shared Successfully!"
          subTitle={
            distance !== null
              ? `Thank you for sharing your location! You are ${distance.toFixed(1)} km away from the rescue location. The admin can now see your proximity and coordinate the rescue more effectively.`
              : 'Thank you for sharing your location! The admin can now see your proximity and coordinate the rescue more effectively.'
          }
          extra={[
            <Button type="primary" key="requests" onClick={() => navigate('/rescue-requests')}>
              View All Rescue Requests
            </Button>,
            <Button key="initiations" onClick={() => navigate('/rescue-initiations')}>
              My Rescue Initiations
            </Button>,
          ]}
        />
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 900, margin: '50px auto', padding: '0 20px' }}>
      <Card>
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div style={{ textAlign: 'center' }}>
            <EnvironmentOutlined style={{ fontSize: 48, color: '#52c41a' }} />
            <Title level={2}>Share Your Location</Title>
            <Paragraph>
              Please select your current location on the map to help us calculate the best route to the rescue location.
              This information helps coordinate faster responses and assign the nearest available rescuer.
            </Paragraph>
          </div>

          <Alert
            message="Why share your location?"
            description={
              <ul style={{ marginBottom: 0, paddingLeft: 20 }}>
                <li>Helps calculate exact distance to the rescue site</li>
                <li>Enables better coordination with other rescuers</li>
                <li>Allows admin to assign the nearest available rescuer</li>
                <li>Provides estimated travel time for planning</li>
                <li>Your location is only used for this specific rescue request</li>
              </ul>
            }
            type="info"
            showIcon
          />

          <LocationPicker
            onLocationSelect={handleLocationShare}
            height={450}
            disabled={loading}
          />

          {loading && (
            <div style={{ textAlign: 'center' }}>
              <Spin /> <Text>Sharing your location...</Text>
            </div>
          )}
        </Space>
      </Card>
    </div>
  );
};

export default ShareLocation;