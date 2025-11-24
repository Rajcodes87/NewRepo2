import React, { useState, useEffect } from 'react';
import {
  Table,
  Button,
  Space,
  Input,
  Select,
  Card,
  Tag,
  Modal,
  Form,
  message,
  Image,
  Descriptions,
  Spin,
  Alert,
} from 'antd';
import { SearchOutlined, EyeOutlined, CheckOutlined, CloseOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import rescuerApplicationService from '../services/rescuerApplicationService';
import { RescuerApplication, RescuerApplicationFilter } from '../types/rescuerApplication';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';

const { Option } = Select;
const { TextArea } = Input;

const RescuerApplicationList: React.FC = () => {
  const [applications, setApplications] = useState<RescuerApplication[]>([]);
  const [loading, setLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [selectedApplication, setSelectedApplication] = useState<RescuerApplication | null>(null);
  const [reviewModalVisible, setReviewModalVisible] = useState(false);
  const [viewModalVisible, setViewModalVisible] = useState(false);
  const [reviewLoading, setReviewLoading] = useState(false);
  const [reviewForm] = Form.useForm();
  const { hasRole } = useAuth();
  const navigate = useNavigate();

  // Check if user is admin
  useEffect(() => {
    if (!hasRole('admin')) {
      message.error('You do not have permission to access this page');
      navigate('/rescue-requests');
    }
  }, [hasRole, navigate]);

  const fetchApplications = async () => {
    setLoading(true);
    try {
      const filter: RescuerApplicationFilter = {
        searchKeyword: searchKeyword || undefined
      };

      const response = await rescuerApplicationService.getList(
        {
          skipCount: (currentPage - 1) * pageSize,
          maxResultCount: pageSize,
          sorting: 'ApplicationDate DESC',
        },
        filter
      );

      if (response.success && response.data) {
        setApplications(response.data.items);
        setTotalCount(response.data.totalCount);
      }
    } catch (error: any) {
      message.error('Failed to load applications');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchApplications();
  }, [currentPage, pageSize, searchKeyword, statusFilter]);

  const handleSearch = () => {
    setCurrentPage(1);
    fetchApplications();
  };

  const handleReset = () => {
    setSearchKeyword('');
    setStatusFilter('');
    setCurrentPage(1);
  };

  const handleViewApplication = (record: RescuerApplication) => {
    setSelectedApplication(record);
    setViewModalVisible(true);
  };

  const handleReviewApplication = (record: RescuerApplication) => {
    setSelectedApplication(record);
    reviewForm.resetFields();
    setReviewModalVisible(true);
  };

  const handleReviewSubmit = async (values: any) => {
    if (!selectedApplication) return;

    setReviewLoading(true);
    try {
      const response = await rescuerApplicationService.reviewApplication({
        applicationId: selectedApplication.id,
        status: values.status,
        reviewNotes: values.reviewNotes,
      });

      if (response.success) {
        message.success(response.message || 'Application reviewed successfully');
        setReviewModalVisible(false);
        fetchApplications();
      } else {
        message.error(response.message || 'Failed to review application');
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.error?.message || 'An error occurred while reviewing the application';
      message.error(errorMessage);
    } finally {
      setReviewLoading(false);
    }
  };

  const getStatusTag = (status: string) => {
    const statusConfig: Record<string, { color: string; text: string }> = {
      Pending: { color: 'orange', text: 'Pending' },
      Approved: { color: 'green', text: 'Approved' },
      Rejected: { color: 'red', text: 'Rejected' },
    };
    const config = statusConfig[status] || { color: 'default', text: status };
    return <Tag color={config.color}>{config.text}</Tag>;
  };

  const columns: ColumnsType<RescuerApplication> = [
    {
      title: 'Application Date',
      dataIndex: 'applicationDate',
      key: 'applicationDate',
      render: (date: string) => dayjs(date).format('YYYY-MM-DD HH:mm'),
      sorter: true,
    },
    {
      title: 'Name',
      key: 'name',
      render: (_, record) => `${record.name} ${record.surname}`,
    },
    {
      title: 'Username',
      dataIndex: 'userName',
      key: 'userName',
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
    },
    {
      title: 'Phone Number',
      dataIndex: 'phoneNumber',
      key: 'phoneNumber',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => getStatusTag(status),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => handleViewApplication(record)}
          >
            View
          </Button>
          {record.status === 'Pending' && (
            <Button
              type="primary"
              size="small"
              onClick={() => handleReviewApplication(record)}
            >
              Review
            </Button>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Card title="Rescuer Applications" style={{ marginBottom: 16 }}>
        <Alert
          message="Review Rescuer Applications"
          description="Review identity card submissions from rescuer applicants. Approve or reject applications based on the identity verification."
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />

        <Space style={{ marginBottom: 16 }}>
          <Input
            placeholder="Search by name, email, or username"
            prefix={<SearchOutlined />}
            value={searchKeyword}
            onChange={(e) => setSearchKeyword(e.target.value)}
            onPressEnter={handleSearch}
            style={{ width: 300 }}
          />
          <Select
            placeholder="Filter by status"
            value={statusFilter}
            onChange={setStatusFilter}
            style={{ width: 150 }}
            allowClear
          >
            <Option value="Pending">Pending</Option>
            <Option value="Approved">Approved</Option>
            <Option value="Rejected">Rejected</Option>
          </Select>
          <Button type="primary" onClick={handleSearch}>
            Search
          </Button>
          <Button onClick={handleReset}>Reset</Button>
        </Space>

        <Table
          columns={columns}
          dataSource={applications}
          rowKey="id"
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: totalCount,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} applications`,
            onChange: (page, size) => {
              setCurrentPage(page);
              setPageSize(size || 10);
            },
          }}
        />
      </Card>

      {/* View Application Modal */}
      <Modal
        title="Application Details"
        open={viewModalVisible}
        onCancel={() => setViewModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setViewModalVisible(false)}>
            Close
          </Button>,
          selectedApplication?.status === 'Pending' && (
            <Button
              key="review"
              type="primary"
              onClick={() => {
                setViewModalVisible(false);
                handleReviewApplication(selectedApplication);
              }}
            >
              Review Application
            </Button>
          ),
        ]}
        width={800}
      >
        {selectedApplication && (
          <div>
            <Descriptions bordered column={2}>
              <Descriptions.Item label="Name">
                {selectedApplication.name} {selectedApplication.surname}
              </Descriptions.Item>
              <Descriptions.Item label="Username">{selectedApplication.userName}</Descriptions.Item>
              <Descriptions.Item label="Email">{selectedApplication.email}</Descriptions.Item>
              <Descriptions.Item label="Phone">{selectedApplication.phoneNumber}</Descriptions.Item>
              <Descriptions.Item label="Status">
                {getStatusTag(selectedApplication.status)}
              </Descriptions.Item>
              <Descriptions.Item label="Application Date">
                {dayjs(selectedApplication.applicationDate).format('YYYY-MM-DD HH:mm')}
              </Descriptions.Item>
              {selectedApplication.reviewedDate && (
                <>
                  <Descriptions.Item label="Reviewed Date">
                    {dayjs(selectedApplication.reviewedDate).format('YYYY-MM-DD HH:mm')}
                  </Descriptions.Item>
                  <Descriptions.Item label="Review Notes" span={2}>
                    {selectedApplication.reviewNotes || 'N/A'}
                  </Descriptions.Item>
                </>
              )}
            </Descriptions>

            <div style={{ marginTop: 24 }}>
              <h3>Identity Card Photo:</h3>
              {selectedApplication.identityCardPicture ? (
                <Image
                  src={selectedApplication.identityCardPicture}
                  alt="Identity Card"
                  style={{ maxWidth: '100%', border: '1px solid #d9d9d9', borderRadius: 4 }}
                />
              ) : (
                <p>No identity card photo uploaded</p>
              )}
            </div>
          </div>
        )}
      </Modal>

      {/* Review Application Modal */}
      <Modal
        title="Review Rescuer Application"
        open={reviewModalVisible}
        onCancel={() => setReviewModalVisible(false)}
        footer={null}
        width={800}
      >
        {selectedApplication && (
          <div>
            <Alert
              message="Review Identity Card"
              description="Carefully verify the identity card photo. If approved, the user will be granted Rescuer role and can participate in rescue operations."
              type="warning"
              showIcon
              style={{ marginBottom: 16 }}
            />

            <div style={{ marginBottom: 24 }}>
              <h3>Applicant Information:</h3>
              <Descriptions bordered column={2} size="small">
                <Descriptions.Item label="Name">
                  {selectedApplication.name} {selectedApplication.surname}
                </Descriptions.Item>
                <Descriptions.Item label="Email">{selectedApplication.email}</Descriptions.Item>
              </Descriptions>
            </div>

            <div style={{ marginBottom: 24 }}>
              <h3>Identity Card Photo:</h3>
              {selectedApplication.identityCardPicture ? (
                <Image
                  src={selectedApplication.identityCardPicture}
                  alt="Identity Card"
                  style={{ maxWidth: '100%', border: '1px solid #d9d9d9', borderRadius: 4 }}
                />
              ) : (
                <p>No identity card photo uploaded</p>
              )}
            </div>

            <Form form={reviewForm} layout="vertical" onFinish={handleReviewSubmit}>
              <Form.Item
                name="status"
                label="Review Decision"
                rules={[{ required: true, message: 'Please select a decision' }]}
              >
                <Select placeholder="Select decision" size="large">
                  <Option value="Approved">
                    <CheckOutlined style={{ color: 'green' }} /> Approve
                  </Option>
                  <Option value="Rejected">
                    <CloseOutlined style={{ color: 'red' }} /> Reject
                  </Option>
                </Select>
              </Form.Item>

              <Form.Item name="reviewNotes" label="Review Notes" rules={[{ max: 1000 }]}>
                <TextArea
                  rows={4}
                  placeholder="Enter any notes or reasons for your decision (optional but recommended for rejections)"
                />
              </Form.Item>

              <Form.Item>
                <Space>
                  <Button type="primary" htmlType="submit" loading={reviewLoading} size="large">
                    Submit Review
                  </Button>
                  <Button onClick={() => setReviewModalVisible(false)} size="large">
                    Cancel
                  </Button>
                </Space>
              </Form.Item>
            </Form>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default RescuerApplicationList;