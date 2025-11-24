import React, { useState, useEffect } from 'react';
import {
  Button,
  Space,
  Input,
  Select,
  Tag,
  Modal,
  message,
  Card,
  Popconfirm,
  Tooltip,
  Image,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  EnvironmentOutlined,
} from '@ant-design/icons';
import { RequestRescueDto, REQUEST_RESCUE_STATUS_OPTIONS } from '../types/requestRescue';
import { useAuth } from '../contexts/AuthContext';
import RequestRescueService from '../services/requestRescueService';
import RequestRescueForm from '../components/RequestRescueForm';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';

const { Search } = Input;
const { Option } = Select;

const RequestRescueList: React.FC = () => {
  const { hasRole } = useAuth();
  const navigate = useNavigate();
  const [data, setData] = useState<RequestRescueDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchKeyword, setSearchKeyword] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [isActiveFilter, setIsActiveFilter] = useState<boolean | undefined>();
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [editingRecord, setEditingRecord] = useState<RequestRescueDto | undefined>();

  // Fetch data
  const fetchData = async () => {
    setLoading(true);
    try {
      const result = await RequestRescueService.getList(
        {
          skipCount: (currentPage - 1) * pageSize,
          maxResultCount: pageSize,
          sorting: 'RequestDate DESC',
        },
        {
          searchKeyword,
          status: statusFilter as any,
          isActive: isActiveFilter,
        }
      );
      setData(result.items);
      setTotal(result.totalCount);
    } catch (error: any) {
      message.error(error.message || 'Failed to fetch rescue requests');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [currentPage, pageSize, searchKeyword, statusFilter, isActiveFilter]);

  // Handle delete
  const handleDelete = async (id: string) => {
    try {
      await RequestRescueService.delete(id);
      message.success('Rescue request deleted successfully');
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to delete rescue request');
    }
  };

  // Handle activate/deactivate
  const handleToggleActive = async (id: string, isActive: boolean) => {
    try {
      if (isActive) {
        await RequestRescueService.deactivate(id);
        message.success('Rescue request deactivated');
      } else {
        await RequestRescueService.activate(id);
        message.success('Rescue request activated');
      }
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to update rescue request');
    }
  };

  // Handle form submit
  const handleFormSubmit = () => {
    setIsFormVisible(false);
    setEditingRecord(undefined);
    fetchData();
  };

  // Get status color
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'NotInitiated':
        return 'default';
      case 'Initiated':
        return 'blue';
      case 'InProgress':
        return 'processing';
      case 'Completed':
        return 'success';
      case 'Cancelled':
        return 'error';
      default:
        return 'default';
    }
  };

  return (
    <div style={{ padding: '24px' }}>
      <Card
        title="Rescue Requests"
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditingRecord(undefined);
              setIsFormVisible(true);
            }}
          >
            New Request
          </Button>
        }
      >
        <Space direction="vertical" style={{ width: '100%', marginBottom: 16 }} size="middle">
          <Space wrap>
            <Search
              placeholder="Search by title, location, contact..."
              allowClear
              style={{ width: 300 }}
              onSearch={setSearchKeyword}
              enterButton={<SearchOutlined />}
            />
            <Select
              placeholder="Filter by Status"
              style={{ width: 150 }}
              allowClear
              onChange={setStatusFilter}
            >
              {REQUEST_RESCUE_STATUS_OPTIONS.map((status) => (
                <Option key={status.value} value={status.value}>
                  {status.label}
                </Option>
              ))}
            </Select>
            <Select
              placeholder="Filter by Active"
              style={{ width: 150 }}
              allowClear
              onChange={setIsActiveFilter}
            >
              <Option value={true}>Active</Option>
              <Option value={false}>Inactive</Option>
            </Select>
          </Space>
        </Space>

        {/* Card layout instead of table */}
        <Space direction="vertical" style={{ width: '100%' }} size="middle">
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '16px' }}>
            {data.map((record) => (
              <Card
                key={record.id}
                hoverable
                style={{ width: '100%' }}
                cover={
                  record.picture ? (
                    <Image
                      alt="Rescue animal"
                      src={record.picture}
                      style={{ height: 225, objectFit: 'cover' }}
                    />
                  ) : (
                    <div
                      style={{
                        width: '100%',
                        height: 225,
                        backgroundColor: '#f0f0f0',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: '#999',
                        fontSize: 16,
                      }}
                    >
                      No Image
                    </div>
                  )
                }
              >
                <Card.Meta
                  title={record.title}
                  description={
                    <div>
                      <div>{record.contactName || 'N/A'}</div>
                      <div style={{ fontSize: '12px', color: '#888' }}>{record.contactNo}</div>
                      <div style={{ fontSize: '12px', color: '#888' }}>
                        {dayjs(record.requestDate).format('MMM DD, YYYY')}
                      </div>
                      <Tag color={getStatusColor(record.status)}>
                        {REQUEST_RESCUE_STATUS_OPTIONS.find(s => s.value === record.status)?.label || record.status}
                      </Tag>
                    </div>
                  }
                />
                <Space size="small" style={{ marginTop: 10 }}>
                  {hasRole('admin') && (
                    <>
                      <Tooltip title="Edit">
                        <Button
                          type="link"
                          icon={<EditOutlined />}
                          onClick={() => {
                            setEditingRecord(record);
                            setIsFormVisible(true);
                          }}
                        />
                      </Tooltip>
                      <Tooltip title="Nearest Rescuers">
                        <Button
                          type="link"
                          icon={<EnvironmentOutlined />}
                          onClick={() => navigate(`/rescue-requests/${record.id}/nearest-rescuers`)}
                        >
                          View Nearest Rescuers
                        </Button>
                      </Tooltip>
                      <Tooltip title={record.isActive ? 'Deactivate' : 'Activate'}>
                        <Button
                          type="link"
                          icon={record.isActive ? <CloseCircleOutlined /> : <CheckCircleOutlined />}
                          onClick={() => handleToggleActive(record.id, record.isActive)}
                        />
                      </Tooltip>
                      <Tooltip title="Delete">
                        <Popconfirm
                          title="Are you sure you want to delete this rescue request?"
                          onConfirm={() => handleDelete(record.id)}
                          okText="Yes"
                          cancelText="No"
                        >
                          <Button type="link" danger icon={<DeleteOutlined />} />
                        </Popconfirm>
                      </Tooltip>
                    </>
                  )}
                  {!hasRole('admin') && (
                    <span style={{ color: '#888', fontSize: '12px' }}>View Only</span>
                  )}
                </Space>
              </Card>
            ))}
          </div>
        </Space>

      </Card>

      <Modal
        title={editingRecord ? 'Edit Rescue Request' : 'New Rescue Request'}
        open={isFormVisible}
        onCancel={() => {
          setIsFormVisible(false);
          setEditingRecord(undefined);
        }}
        footer={null}
        width={700}
        destroyOnClose
      >
        <RequestRescueForm
          editingRecord={editingRecord}
          onSubmit={handleFormSubmit}
          onCancel={() => {
            setIsFormVisible(false);
            setEditingRecord(undefined);
          }}
        />
      </Modal>
    </div>
  );
};

export default RequestRescueList;
