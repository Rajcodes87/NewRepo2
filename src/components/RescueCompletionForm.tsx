import React, { useEffect, useState } from 'react';
import { Form, Input, Button, Space, message, Select, DatePicker, Upload, Image } from 'antd';
import { UploadOutlined, DeleteOutlined } from '@ant-design/icons';
import type { UploadFile, UploadProps } from 'antd';
import {
  CreateRescueCompletionDto,
  UpdateRescueCompletionDto,
  RescueCompletionDto,
} from '../types/rescueCompletion';
import { RequestRescueDto } from '../types/requestRescue';
import RescueCompletionService from '../services/rescueCompletionService';
import RequestRescueService from '../services/requestRescueService';
import dayjs from 'dayjs';

const { TextArea } = Input;
const { Option } = Select;

interface RescueCompletionFormProps {
  editingRecord?: RescueCompletionDto;
  onSubmit: () => void;
  onCancel: () => void;
}

const RescueCompletionForm: React.FC<RescueCompletionFormProps> = ({
  editingRecord,
  onSubmit,
  onCancel,
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [requests, setRequests] = useState<RequestRescueDto[]>([]);
  const [loadingRequests, setLoadingRequests] = useState(false);
  const [imageBase64, setImageBase64] = useState<string | null>(null);
  const [fileList, setFileList] = useState<UploadFile[]>([]);

  useEffect(() => {
    // Load available rescue requests if creating new completion
    if (!editingRecord) {
      fetchAvailableRequests();
    }

    if (editingRecord) {
      form.setFieldsValue({
        completionDate: editingRecord.completionDate ? dayjs(editingRecord.completionDate) : null,
        completionDescription: editingRecord.completionDescription,
      });

      // Set existing image if present
      if (editingRecord.completionProofPicture) {
        setImageBase64(editingRecord.completionProofPicture);
        setFileList([{
          uid: '-1',
          name: 'proof.jpg',
          status: 'done',
          url: editingRecord.completionProofPicture,
        }]);
      }
    }
  }, [editingRecord, form]);

  const fetchAvailableRequests = async () => {
    setLoadingRequests(true);
    try {
      const result = await RequestRescueService.getList(
        {
          skipCount: 0,
          maxResultCount: 100,
        },
        {
          isActive: true,
          status: 'InProgress',
        }
      );
      setRequests(result.items);
    } catch (error: any) {
      message.error(error.message || 'Failed to fetch rescue requests');
    } finally {
      setLoadingRequests(false);
    }
  };

  // Convert file to base64
  const convertToBase64 = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = (error) => reject(error);
    });
  };

  // Handle image upload
  const handleImageUpload: UploadProps['beforeUpload'] = async (file) => {
    const isImage = file.type.startsWith('image/');
    if (!isImage) {
      message.error('You can only upload image files!');
      return Upload.LIST_IGNORE;
    }

    const isLt5M = file.size / 1024 / 1024 < 5;
    if (!isLt5M) {
      message.error('Image must be smaller than 5MB!');
      return Upload.LIST_IGNORE;
    }

    try {
      const base64 = await convertToBase64(file);
      setImageBase64(base64);
      setFileList([{
        uid: file.uid,
        name: file.name,
        status: 'done',
        url: base64,
      }]);
      message.success('Image uploaded successfully');
    } catch (error) {
      message.error('Failed to upload image');
    }

    return false; // Prevent automatic upload
  };

  // Handle image removal
  const handleImageRemove = () => {
    setImageBase64(null);
    setFileList([]);
  };

  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      if (editingRecord) {
        const updateDto: UpdateRescueCompletionDto = {
          completionProofPicture: imageBase64 || undefined,
          completionDate: values.completionDate.toISOString(),
          completionDescription: values.completionDescription,
        };
        await RescueCompletionService.update(editingRecord.id, updateDto);
        message.success('Rescue completion updated successfully');
      } else {
        const createDto: CreateRescueCompletionDto = {
          requestRescueId: values.requestRescueId,
          completionProofPicture: imageBase64 || undefined,
          completionDate: values.completionDate.toISOString(),
          completionDescription: values.completionDescription,
        };
        await RescueCompletionService.create(createDto);
        message.success('Rescue completion created successfully');
      }
      form.resetFields();
      setImageBase64(null);
      setFileList([]);
      onSubmit();
    } catch (error: any) {
      message.error(error.message || 'Failed to save rescue completion');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Form
      form={form}
      layout="vertical"
      onFinish={handleSubmit}
      initialValues={{
        completionDate: dayjs(),
      }}
    >
      {!editingRecord && (
        <Form.Item
          label="Rescue Request"
          name="requestRescueId"
          rules={[{ required: true, message: 'Please select a rescue request' }]}
        >
          <Select
            placeholder="Select a rescue request"
            loading={loadingRequests}
            showSearch
            optionFilterProp="children"
          >
            {requests.map((request) => (
              <Option key={request.id} value={request.id}>
                {request.title} - {request.location}
              </Option>
            ))}
          </Select>
        </Form.Item>
      )}

      {editingRecord && (
        <Form.Item label="Rescue Request">
          <div style={{ padding: '8px 12px', background: '#f5f5f5', borderRadius: '4px' }}>
            <strong>{editingRecord.requestTitle}</strong>
            <div style={{ fontSize: '12px', color: '#888' }}>{editingRecord.requestLocation}</div>
          </div>
        </Form.Item>
      )}

      <Form.Item
        label="Completion Date"
        name="completionDate"
        rules={[{ required: true, message: 'Please select completion date' }]}
      >
        <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
      </Form.Item>

      <Form.Item label="Proof Picture (Optional)">
        <Space direction="vertical" style={{ width: '100%' }}>
          <Upload
            listType="picture"
            fileList={fileList}
            beforeUpload={handleImageUpload}
            onRemove={handleImageRemove}
            maxCount={1}
          >
            {fileList.length === 0 && (
              <Button icon={<UploadOutlined />}>Upload Proof Picture</Button>
            )}
          </Upload>
          {imageBase64 && (
            <div style={{ marginTop: 16 }}>
              <div style={{ marginBottom: 8 }}>
                <strong>Preview:</strong>
              </div>
              <Image
                src={imageBase64}
                alt="Proof"
                style={{ maxWidth: '100%', maxHeight: 300, objectFit: 'contain' }}
              />
              <Button
                danger
                icon={<DeleteOutlined />}
                onClick={handleImageRemove}
                style={{ marginTop: 8 }}
              >
                Remove Picture
              </Button>
            </div>
          )}
          <div style={{ fontSize: '12px', color: '#888' }}>
            Upload proof of rescue completion (Max 5MB)
          </div>
        </Space>
      </Form.Item>

      <Form.Item
        label="Completion Description"
        name="completionDescription"
        rules={[{ max: 2000, message: 'Description cannot exceed 2000 characters' }]}
      >
        <TextArea
          rows={4}
          placeholder="Describe the completion and outcome"
          showCount
          maxLength={2000}
        />
      </Form.Item>

      <Form.Item>
        <Space>
          <Button type="primary" htmlType="submit" loading={loading}>
            {editingRecord ? 'Update' : 'Create'}
          </Button>
          <Button onClick={onCancel}>Cancel</Button>
        </Space>
      </Form.Item>
    </Form>
  );
};

export default RescueCompletionForm;
