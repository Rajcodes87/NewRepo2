import React, { useState, useEffect } from 'react';
import {
  Table,
  Button,
  Space,
  Input,
  Select,
  Modal,
  message,
  Card,
  Popconfirm,
  Tooltip,
  Form,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  SearchOutlined,
  SafetyOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { RescueCompletionDto, VerifyCompletionDto } from '../types/rescueCompletion';
import RescueCompletionService from '../services/rescueCompletionService';
import RescueCompletionForm from '../components/RescueCompletionForm';
import { useAuth } from '../contexts/AuthContext';
import dayjs from 'dayjs';

const { Search } = Input;
const { Option } = Select;
const { TextArea } = Input;

const RescueCompletionList: React.FC = () => {
  const { hasRole } = useAuth();
  const [data, setData] = useState<RescueCompletionDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchKeyword, setSearchKeyword] = useState<string>('');
  const [isVerifiedFilter, setIsVerifiedFilter] = useState<boolean | undefined>();
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [isVerifyModalVisible, setIsVerifyModalVisible] = useState(false);
  const [editingRecord, setEditingRecord] = useState<RescueCompletionDto | undefined>();
  const [verifyingRecord, setVerifyingRecord] = useState<RescueCompletionDto | undefined>();
  const [verifyForm] = Form.useForm();

  // Fetch data
  const fetchData = async () => {
    setLoading(true);
    try {
      const result = await RescueCompletionService.getList(
        {
          skipCount: (currentPage - 1) * pageSize,
          maxResultCount: pageSize,
          sorting: 'CompletionDate DESC',
        },
        {
          searchKeyword,
          isVerified: isVerifiedFilter,
        }
      );
      setData(result.items);
      setTotal(result.totalCount);
    } catch (error: any) {
      message.error(error.message || 'Failed to fetch rescue completions');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [currentPage, pageSize, searchKeyword, isVerifiedFilter]);

  // Handle delete
  const handleDelete = async (id: string) => {
    try {
      await RescueCompletionService.delete(id);
      message.success('Rescue completion deleted successfully');
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to delete rescue completion');
    }
  };

  // Handle verify
  const handleVerifySubmit = async (values: VerifyCompletionDto) => {
    if (!verifyingRecord) return;

    try {
      await RescueCompletionService.verify(verifyingRecord.id, values);
      message.success('Rescue completion verified successfully');
      setIsVerifyModalVisible(false);
      setVerifyingRecord(undefined);
      verifyForm.resetFields();
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to verify rescue completion');
    }
  };

  // Handle unverify
  const handleUnverify = async (id: string) => {
    try {
      await RescueCompletionService.unverify(id);
      message.success('Rescue completion unverified');
      fetchData();
    } catch (error: any) {
      message.error(error.message || 'Failed to unverify rescue completion');
    }
  };

  // Handle form submit
  const handleFormSubmit = () => {
    setIsFormVisible(false);
    setEditingRecord(undefined);
    fetchData();
  };

  // Table columns
  const columns: ColumnsType<RescueCompletionDto> = [
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
      title: 'Completed By',
      key: 'completedBy',
      width: 150,
      render: (_, record) => (
        <div>
          <div>{record.completedByRescuerName || 'N/A'}</div>
          <div style={{ fontSize: '12px', color: '#888' }}>{record.completedByRescuerEmail}</div>
        </div>
      ),
    },
    {
      title: 'Completion Date',
      dataIndex: 'completionDate',
      key: 'completionDate',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
    },
    {
      title: 'Verified',
      dataIndex: 'isVerified',
      key: 'isVerified',
      width: 80,
      align: 'center',
      render: (isVerified: boolean) =>
        isVerified ? (
          <CheckCircleOutlined style={{ color: 'green', fontSize: 18 }} />
        ) : (
          <CloseCircleOutlined style={{ color: '#ddd', fontSize: 18 }} />
        ),
    },
    {
      title: 'Verified By',
      key: 'verifiedBy',
      width: 150,
      render: (_, record) =>
        record.isVerified ? (
          <div>
            <div>{record.verifiedByUserName || 'N/A'}</div>
            <div style={{ fontSize: '12px', color: '#888' }}>
              {record.verifiedDate ? dayjs(record.verifiedDate).format('MMM DD, YYYY') : ''}
            </div>
          </div>
        ) : (
          '-'
        ),
    },
    {
      title: 'Description',
      dataIndex: 'completionDescription',
      key: 'completionDescription',
      width: 200,
      ellipsis: true,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 200,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          {!record.isVerified && (
            <>
              {/* Only admins can verify */}
              {hasRole('admin') && (
                <Tooltip title="Verify">
                  <Button
                    type="link"
                    icon={<SafetyOutlined />}
                    onClick={() => {
                      setVerifyingRecord(record);
                      setIsVerifyModalVisible(true);
                    }}
                  />
                </Tooltip>
              )}
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
              <Tooltip title="Delete">
                <Popconfirm
                  title="Are you sure you want to delete this completion?"
                  onConfirm={() => handleDelete(record.id)}
                  okText="Yes"
                  cancelText="No"
                >
                  <Button type="link" danger icon={<DeleteOutlined />} />
                </Popconfirm>
              </Tooltip>
            </>
          )}
          {/* Only admins can unverify */}
          {record.isVerified && hasRole('admin') && (
            <Tooltip title="Unverify">
              <Popconfirm
                title="Are you sure you want to unverify this completion?"
                onConfirm={() => handleUnverify(record.id)}
                okText="Yes"
                cancelText="No"
              >
                <Button type="link" icon={<CloseCircleOutlined />} />
              </Popconfirm>
            </Tooltip>
          )}
          {record.isVerified && !hasRole('admin') && (
            <span style={{ color: '#888', fontSize: '12px' }}>Verified</span>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Card
        title="Rescue Completions"
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditingRecord(undefined);
              setIsFormVisible(true);
            }}
          >
            New Completion
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
              placeholder="Filter by Verified"
              style={{ width: 150 }}
              allowClear
              onChange={setIsVerifiedFilter}
            >
              <Option value={true}>Verified</Option>
              <Option value={false}>Not Verified</Option>
            </Select>
          </Space>
        </Space>

        <Table
          columns={columns}
          dataSource={data}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1300 }}
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
        title={editingRecord ? 'Edit Rescue Completion' : 'New Rescue Completion'}
        open={isFormVisible}
        onCancel={() => {
          setIsFormVisible(false);
          setEditingRecord(undefined);
        }}
        footer={null}
        width={700}
        destroyOnClose
      >
        <RescueCompletionForm
          editingRecord={editingRecord}
          onSubmit={handleFormSubmit}
          onCancel={() => {
            setIsFormVisible(false);
            setEditingRecord(undefined);
          }}
        />
      </Modal>

      <Modal
        title="Verify Rescue Completion"
        open={isVerifyModalVisible}
        onCancel={() => {
          setIsVerifyModalVisible(false);
          setVerifyingRecord(undefined);
          verifyForm.resetFields();
        }}
        footer={null}
        width={600}
        destroyOnClose
      >
        <Form form={verifyForm} layout="vertical" onFinish={handleVerifySubmit}>
          {verifyingRecord && (
            <div style={{ marginBottom: 16, padding: 12, background: '#f5f5f5', borderRadius: 4 }}>
              <strong>{verifyingRecord.requestTitle}</strong>
              <div style={{ fontSize: '12px', color: '#888' }}>
                Completed by: {verifyingRecord.completedByRescuerName}
              </div>
              <div style={{ fontSize: '12px', color: '#888' }}>
                Date: {dayjs(verifyingRecord.completionDate).format('MMM DD, YYYY')}
              </div>
            </div>
          )}

          <Form.Item
            label="Verification Notes"
            name="verificationNotes"
            rules={[{ max: 1000, message: 'Notes cannot exceed 1000 characters' }]}
          >
            <TextArea
              rows={4}
              placeholder="Add any verification notes (optional)"
              showCount
              maxLength={1000}
            />
          </Form.Item>

          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit">
                Verify
              </Button>
              <Button
                onClick={() => {
                  setIsVerifyModalVisible(false);
                  setVerifyingRecord(undefined);
                  verifyForm.resetFields();
                }}
              >
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default RescueCompletionList;
