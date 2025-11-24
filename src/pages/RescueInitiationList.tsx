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
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  SearchOutlined,
  RollbackOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { RescueInitiationDto, RESCUE_INITIATION_STATUS_OPTIONS } from '../types/rescueInitiation';
import RescueInitiationService from '../services/rescueInitiationService';
import RescueInitiationForm from '../components/RescueInitiationForm';
import dayjs from 'dayjs';

const { Search } = Input;
const { Option } = Select;

const RescueInitiationList: React.FC = () => {
  const [data, setData] = useState<RescueInitiationDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchKeyword, setSearchKeyword] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [editingRecord, setEditingRecord] = useState<RescueInitiationDto | undefined>();

  // Fetch data
  const fetchData = async () => {
    setLoading(true);
    try {
      const result = await RescueInitiationService.getList(
        {
          skipCount: (currentPage - 1) * pageSize,
          maxResultCount: pageSize,
          sorting: 'InitiatedDate DESC',
        },
        {
          searchKeyword,
          status: statusFilter as any,
        }
      );
      setData(result.items);
      setTotal(result.totalCount);
    } catch (error: any) {
      message.error(error.message || 'Failed to fetch rescue initiations');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [currentPage, pageSize, searchKeyword, statusFilter]);

  // Handle delete
  const handleDelete = async (id: string) => {
    try {
      await RescueInitiationService.delete(id);
      message.success('Rescue initiation deleted successfully');
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to delete rescue initiation');
    }
  };

  // Handle accept
  const handleAccept = async (id: string) => {
    try {
      await RescueInitiationService.accept(id);
      message.success('Rescue initiation accepted');
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to accept rescue initiation');
    }
  };

  // Handle reject
  const handleReject = async (id: string) => {
    try {
      await RescueInitiationService.reject(id);
      message.success('Rescue initiation rejected');
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to reject rescue initiation');
    }
  };

  // Handle withdraw
  const handleWithdraw = async (id: string) => {
    try {
      await RescueInitiationService.withdraw(id);
      message.success('Rescue initiation withdrawn');
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to withdraw rescue initiation');
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
      case 'Pending':
        return 'processing';
      case 'Accepted':
        return 'success';
      case 'Rejected':
        return 'error';
      case 'Withdrawn':
        return 'default';
      default:
        return 'default';
    }
  };

  // Table columns
  const columns: ColumnsType<RescueInitiationDto> = [
    {
      title: 'Request Title',
      dataIndex: 'requestTitle',
      key: 'requestTitle',
      width: 200,
      ellipsis: true,
    },
    {
      title: 'Location',
      dataIndex: 'requestLocation',
      key: 'requestLocation',
      width: 150,
      ellipsis: true,
    },
    {
      title: 'Rescuer',
      key: 'rescuer',
      width: 150,
      render: (_, record) => (
        <div>
          <div>{record.rescuerName || 'N/A'}</div>
          <div style={{ fontSize: '12px', color: '#888' }}>{record.rescuerEmail}</div>
        </div>
      ),
    },
    {
      title: 'Initiated Date',
      dataIndex: 'initiatedDate',
      key: 'initiatedDate',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status: string) => (
        <Tag color={getStatusColor(status)}>
          {RESCUE_INITIATION_STATUS_OPTIONS.find(s => s.value === status)?.label || status}
        </Tag>
      ),
    },
    {
      title: 'Selected',
      dataIndex: 'isSelected',
      key: 'isSelected',
      width: 80,
      align: 'center',
      render: (isSelected: boolean) =>
        isSelected ? (
          <CheckCircleOutlined style={{ color: 'green', fontSize: 18 }} />
        ) : (
          <CloseCircleOutlined style={{ color: '#ddd', fontSize: 18 }} />
        ),
    },
    {
      title: 'Notes',
      dataIndex: 'notes',
      key: 'notes',
      width: 200,
      ellipsis: true,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 220,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          {record.status === 'Pending' && (
            <>
              <Tooltip title="Accept">
                <Popconfirm
                  title="Accept this initiation? This will reject all other pending initiations."
                  onConfirm={() => handleAccept(record.id)}
                  okText="Yes"
                  cancelText="No"
                >
                  <Button type="link" icon={<CheckCircleOutlined />} />
                </Popconfirm>
              </Tooltip>
              <Tooltip title="Reject">
                <Popconfirm
                  title="Are you sure you want to reject this initiation?"
                  onConfirm={() => handleReject(record.id)}
                  okText="Yes"
                  cancelText="No"
                >
                  <Button type="link" danger icon={<CloseCircleOutlined />} />
                </Popconfirm>
              </Tooltip>
              <Tooltip title="Withdraw">
                <Button
                  type="link"
                  icon={<RollbackOutlined />}
                  onClick={() => handleWithdraw(record.id)}
                />
              </Tooltip>
            </>
          )}
          {record.status !== 'Accepted' && (
            <>
              <Tooltip title="Edit">
                <Button
                  type="link"
                  icon={<EditOutlined />}
                  onClick={() => {
                    setEditingRecord(record);
                    setIsFormVisible(true);
                  }}
                  disabled={record.status !== 'Pending'}
                />
              </Tooltip>
              <Tooltip title="Delete">
                <Popconfirm
                  title="Are you sure you want to delete this initiation?"
                  onConfirm={() => handleDelete(record.id)}
                  okText="Yes"
                  cancelText="No"
                >
                  <Button
                    type="link"
                    danger
                    icon={<DeleteOutlined />}
                  />
                </Popconfirm>
              </Tooltip>
            </>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Card
        title="Rescue Initiations"
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditingRecord(undefined);
              setIsFormVisible(true);
            }}
          >
            New Initiation
          </Button>
        }
      >
        <Space direction="vertical" style={{ width: '100%', marginBottom: 16 }} size="middle">
          <Space wrap>
            <Search
              placeholder="Search by request title, rescuer..."
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
              {RESCUE_INITIATION_STATUS_OPTIONS.map((status) => (
                <Option key={status.value} value={status.value}>
                  {status.label}
                </Option>
              ))}
            </Select>
          </Space>
        </Space>

        <Table
          columns={columns}
          dataSource={data}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1200 }}
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
        title={editingRecord ? 'Edit Rescue Initiation' : 'New Rescue Initiation'}
        open={isFormVisible}
        onCancel={() => {
          setIsFormVisible(false);
          setEditingRecord(undefined);
        }}
        footer={null}
        width={600}
        destroyOnClose
      >
        <RescueInitiationForm
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

export default RescueInitiationList;
