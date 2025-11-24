import React, { useState, useEffect } from 'react';
import {
  Table,
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
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { RequestRescueDto, REQUEST_RESCUE_STATUS_OPTIONS } from '../types/requestRescue';
import { useAuth } from '../contexts/AuthContext';
import RequestRescueService from '../services/requestRescueService';
import RequestRescueForm from '../components/RequestRescueForm';
import dayjs from 'dayjs';

const { Search } = Input;
const { Option } = Select;

const RequestRescueList: React.FC = () => {
  const { hasRole } = useAuth();
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

  // Table columns
  const columns: ColumnsType<RequestRescueDto> = [
    {
      title: 'Picture',
      dataIndex: 'picture',
      key: 'picture',
      width: 600,
      align: 'center',
      render: (picture: string) => (
        picture ? (
          <Image
            src={picture}
            alt="Rescue animal"
            width={450}
            height={225}
            style={{ objectFit: 'cover', borderRadius: 8 }}
            preview={{
              mask: 'View',
            }}
          />
        ) : (
          <div style={{
            width: 450,
            height: 225,
            backgroundColor: '#f0f0f0',
            borderRadius: 8,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#999',
            fontSize: 16
          }}>
            No Image
          </div>
        )
      ),
    },
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
      width: 200,
      ellipsis: true,
    },
    {
      title: 'Location',
      dataIndex: 'location',
      key: 'location',
      width: 150,
      ellipsis: true,
    },
    {
      title: 'Contact',
      key: 'contact',
      width: 150,
      render: (_, record) => (
        <div>
          <div>{record.contactName || 'N/A'}</div>
          <div style={{ fontSize: '12px', color: '#888' }}>{record.contactNo}</div>
        </div>
      ),
    },
    {
      title: 'Request Date',
      dataIndex: 'requestDate',
      key: 'requestDate',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (status: string) => (
        <Tag color={getStatusColor(status)}>
          {REQUEST_RESCUE_STATUS_OPTIONS.find(s => s.value === status)?.label || status}
        </Tag>
      ),
    },
    {
      title: 'Initiations',
      dataIndex: 'initiationsCount',
      key: 'initiationsCount',
      width: 100,
      align: 'center',
    },
    {
      title: 'Active',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 80,
      align: 'center',
      render: (isActive: boolean) =>
        isActive ? (
          <CheckCircleOutlined style={{ color: 'green', fontSize: 18 }} />
        ) : (
          <CloseCircleOutlined style={{ color: 'red', fontSize: 18 }} />
        ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 200,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          {/* Only admins can edit, activate/deactivate, and delete */}
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
      ),
    },
  ];

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

        <Table
          columns={columns}
          dataSource={data}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1500 }}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: total,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} items`,
            onChange: (page, size) => {
              setCurrentPage(page);
              setPageSize(size);
            },
          }}
        />
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
