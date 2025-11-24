import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Card,
  Table,
  Typography,
  Button,
  Space,
  Tag,
  message,
  Spin,
  Alert,
  Divider,
  Descriptions,
  Empty,
} from 'antd';
import {
  EnvironmentOutlined,
  UserOutlined,
  MailOutlined,
  ArrowLeftOutlined,
  ReloadOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import RescuerProfileService from '../services/rescuerProfileService';
import RequestRescueService from '../services/requestRescueService';
import type { RescuerDistanceDto } from '../types/rescuerProfile';
import type { RequestRescueDto } from '../types/requestRescue';

const { Title, Text, Paragraph } = Typography;

const NearestRescuersView: React.FC = () => {
  const { requestId } = useParams<{ requestId: string }>();
  const navigate = useNavigate();
  
  const [loading, setLoading] = useState(true);
  const [request, setRequest] = useState<RequestRescueDto | null>(null);
  const [rescuers, setRescuers] = useState<RescuerDistanceDto[]>([]);
  const [selectedRescuer, setSelectedRescuer] = useState<string | null>(null);

  useEffect(() => {
    if (requestId) {
      fetchData();
    }
  }, [requestId]);

  const fetchData = async () => {
    if (!requestId) return;

    setLoading(true);
    try {
      // Fetch rescue request details
      const requestResponse = await RequestRescueService.get(requestId);
      setRequest(requestResponse);

      // Fetch nearest rescuers
      try {
        const rescuersResponse = await RescuerProfileService.getNearestRescuers(requestId, 20);
        setRescuers(rescuersResponse);
        
        if (rescuersResponse.length === 0) {
          message.info('No rescuers have shared their location yet.');
        }
      } catch (rescuerError: any) {
        // Handle case where no rescuers found or request has no location
        console.warn('No nearest rescuers found:', rescuerError);
        setRescuers([]);
      }
    } catch (error: any) {
      console.error('Error fetching data:', error);
      message.error('Failed to load rescue request details');
    } finally {
      setLoading(false);
    }
  };

  const handleAssignRescuer = (rescuerId: string) => {
    // TODO: Implement assign rescuer functionality
    // This would create a RescueInitiation and mark it as accepted
    message.info(`Assign feature coming soon! Selected rescuer: ${rescuerId}`);
    setSelectedRescuer(rescuerId);
  };

  const handleRefresh = () => {
    fetchData();
    message.success('Data refreshed!');
  };

  const columns: ColumnsType<RescuerDistanceDto> = [
    {
      title: 'Rank',
      key: 'rank',
      width: 60,
      render: (_: any, __: any, index: number) => (
        <Tag color={index === 0 ? 'gold' : index === 1 ? 'silver' : index === 2 ? 'orange' : 'default'}>
          #{index + 1}
        </Tag>
      ),
    },
    {
      title: 'Rescuer Name',
      dataIndex: 'rescuerName',
      key: 'rescuerName',
      render: (name: string) => (
        <Space>
          <UserOutlined />
          <Text strong>{name}</Text>
        </Space>
      ),
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
      render: (email: string) => (
        <Space>
          <MailOutlined />
          <Text copyable>{email}</Text>
        </Space>
      ),
    },
    {
      title: 'Distance',
      dataIndex: 'distanceDisplay',
      key: 'distance',
      sorter: (a, b) => a.distance - b.distance,
      render: (distanceDisplay: string, record: RescuerDistanceDto) => (
        <Space>
          <EnvironmentOutlined style={{ color: '#52c41a' }} />
          <Text strong style={{ color: record.distance < 5 ? '#52c41a' : record.distance < 10 ? '#faad14' : '#ff4d4f' }}>
            {distanceDisplay}
          </Text>
        </Space>
      ),
    },
    {
      title: 'Action',
      key: 'action',
      render: (_: any, record: RescuerDistanceDto) => (
        <Button
          type={selectedRescuer === record.rescuerId ? 'default' : 'primary'}
          icon={selectedRescuer === record.rescuerId ? <CheckCircleOutlined /> : undefined}
          onClick={() => handleAssignRescuer(record.rescuerId)}
          disabled={selectedRescuer !== null && selectedRescuer !== record.rescuerId}
        >
          {selectedRescuer === record.rescuerId ? 'Selected' : 'Assign'}
        </Button>
      ),
    },
  ];

  if (loading) {
    return (
      <div style={{ maxWidth: 1200, margin: '50px auto', padding: '0 20px', textAlign: 'center' }}>
        <Spin size="large" />
        <Paragraph style={{ marginTop: 20 }}>Loading nearest rescuers...</Paragraph>
      </div>
    );
  }

  if (!request) {
    return (
      <div style={{ maxWidth: 1200, margin: '50px auto', padding: '0 20px' }}>
        <Alert
          message="Request Not Found"
          description="The requested rescue request could not be found."
          type="error"
          showIcon
        />
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 1200, margin: '20px auto', padding: '0 20px' }}>
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        {/* Header */}
        <Card>
          <Space style={{ width: '100%', justifyContent: 'space-between' }}>
            <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/rescue-requests')}>
              Back to Requests
            </Button>
            <Button icon={<ReloadOutlined />} onClick={handleRefresh}>
              Refresh
            </Button>
          </Space>
        </Card>

        {/* Rescue Request Details */}
        <Card title={<Title level={3}>📍 Rescue Request Details</Title>}>
          <Descriptions bordered column={2}>
            <Descriptions.Item label="Title" span={2}>
              <Text strong>{request.title}</Text>
            </Descriptions.Item>
            <Descriptions.Item label="Location" span={2}>
              <Space>
                <EnvironmentOutlined />
                {request.location}
              </Space>
            </Descriptions.Item>
            <Descriptions.Item label="Description" span={2}>
              {request.description}
            </Descriptions.Item>
            <Descriptions.Item label="Severity">
              <Tag color={
                request.severity === 'Critical' ? 'red' :
                request.severity === 'High' ? 'orange' :
                request.severity === 'Medium' ? 'gold' : 'green'
              }>
                {request.severity}
              </Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Status">
              <Tag color={request.status === 'Completed' ? 'success' : 'processing'}>
                {request.status}
              </Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Contact">
              {request.contactNo} {request.contactName && `(${request.contactName})`}
            </Descriptions.Item>
            <Descriptions.Item label="Request Date">
              {new Date(request.requestDate).toLocaleString()}
            </Descriptions.Item>
          </Descriptions>

          {request.latitude && request.longitude && (
            <>
              <Divider />
              <Space>
                <Button
                  type="link"
                  icon={<EnvironmentOutlined />}
                  href={`https://www.openstreetmap.org/?mlat=${request.latitude}&mlon=${request.longitude}#map=15/${request.latitude}/${request.longitude}`}
                  target="_blank"
                >
                  View on OpenStreetMap
                </Button>
                <Button
                  type="link"
                  icon={<EnvironmentOutlined />}
                  href={`https://www.google.com/maps?q=${request.latitude},${request.longitude}`}
                  target="_blank"
                >
                  View on Google Maps
                </Button>
              </Space>
            </>
          )}
        </Card>

        {/* Nearest Rescuers Table */}
        <Card
          title={
            <Space>
              <Title level={3} style={{ margin: 0 }}>
                🚀 Nearest Rescuers ({rescuers.length})
              </Title>
              {rescuers.length > 0 && rescuers[0].distance < 5 && (
                <Tag color="success">Rescuer Very Close!</Tag>
              )}
            </Space>
          }
        >
          {rescuers.length === 0 ? (
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={
                <Space direction="vertical">
                  <Text>No rescuers have shared their location yet.</Text>
                  <Text type="secondary">
                    Rescuers will appear here once they click the location sharing link from their email.
                  </Text>
                </Space>
              }
            />
          ) : (
            <>
              <Alert
                message="How to use this dashboard"
                description={
                  <ul style={{ marginBottom: 0, paddingLeft: 20 }}>
                    <li>Rescuers are automatically sorted by distance (nearest first)</li>
                    <li>The top 3 rescuers are highlighted with badges</li>
                    <li>Green distance means less than 5km away (very close)</li>
                    <li>Yellow distance means 5-10km away (moderate)</li>
                    <li>Red distance means more than 10km away (far)</li>
                    <li>Click "Assign" to select a rescuer for this rescue</li>
                  </ul>
                }
                type="info"
                showIcon
                style={{ marginBottom: 16 }}
              />

              <Table
                columns={columns}
                dataSource={rescuers}
                rowKey="rescuerId"
                pagination={{ pageSize: 10 }}
                rowClassName={(record, index) =>
                  index === 0 ? 'nearest-rescuer-row' :
                  selectedRescuer === record.rescuerId ? 'selected-rescuer-row' : ''
                }
              />
            </>
          )}
        </Card>

        {/* Tips Card */}
        <Card title="💡 Tips">
          <Space direction="vertical">
            <Text>
              <strong>Refresh the page</strong> to see updated rescuer locations as more rescuers share their location.
            </Text>
            <Text>
              The distance calculation uses the <strong>Haversine formula</strong> for accurate geographic distance.
            </Text>
            <Text>
              Consider factors like <strong>traffic, road conditions, and rescuer availability</strong> when assigning.
            </Text>
          </Space>
        </Card>
      </Space>

      <style>{`
        .nearest-rescuer-row {
          background-color: #f6ffed !important;
          border-left: 3px solid #52c41a;
        }
        .selected-rescuer-row {
          background-color: #e6f7ff !important;
        }
      `}</style>
    </div>
  );
};

export default NearestRescuersView;