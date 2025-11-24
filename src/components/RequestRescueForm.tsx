import React, { useEffect, useState } from 'react';
import { Form, Input, Button, Space, message, Switch, Upload } from 'antd';
import { UploadOutlined, DeleteOutlined } from '@ant-design/icons';
import type { UploadFile, UploadProps } from 'antd';
import {
  CreateUpdateRequestRescueDto,
  RequestRescueDto,
} from '../types/requestRescue';
import RequestRescueService from '../services/requestRescueService';

const { TextArea } = Input;

interface RequestRescueFormProps {
  editingRecord?: RequestRescueDto;
  onSubmit: () => void;
  onCancel: () => void;
}

const RequestRescueForm: React.FC<RequestRescueFormProps> = ({
  editingRecord,
  onSubmit,
  onCancel,
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [imageBase64, setImageBase64] = useState<string | null>(null);
  const [fileList, setFileList] = useState<UploadFile[]>([]);

  useEffect(() => {
    if (editingRecord) {
      form.setFieldsValue({
        title: editingRecord.title,
        location: editingRecord.location,
        description: editingRecord.description,
        contactNo: editingRecord.contactNo,
        contactName: editingRecord.contactName,
        isActive: editingRecord.isActive,
      });

      // Set existing image if present
      if (editingRecord.picture) {
        setImageBase64(editingRecord.picture);
        setFileList([{
          uid: '-1',
          name: 'image.jpg',
          status: 'done',
          url: editingRecord.picture,
        }]);
      }
    } else {
      form.setFieldsValue({
        isActive: true,
      });
    }
  }, [editingRecord, form]);

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

  const handleSubmit = async (values: CreateUpdateRequestRescueDto) => {
    setLoading(true);
    try {
      // Include base64 image in the submission
      const submitData = {
        ...values,
        picture: imageBase64 || undefined,
      };

      if (editingRecord) {
        await RequestRescueService.update(editingRecord.id, submitData);
        message.success('Rescue request updated successfully');
      } else {
        await RequestRescueService.create(submitData);
        message.success('Rescue request created successfully');
      }
      form.resetFields();
      setImageBase64(null);
      setFileList([]);
      onSubmit();
    } catch (error: any) {
      message.error(error.message || 'Failed to save rescue request');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Form
      form={form}
      layout="vertical"
      onFinish={handleSubmit}
      initialValues={{ isActive: true }}
    >
      <Form.Item
        label="Title"
        name="title"
        rules={[
          { required: true, message: 'Please enter title' },
          { max: 256, message: 'Title cannot exceed 256 characters' },
        ]}
      >
        <Input placeholder="Enter rescue request title" />
      </Form.Item>

      <Form.Item
        label="Location"
        name="location"
        rules={[
          { required: true, message: 'Please enter location' },
          { max: 512, message: 'Location cannot exceed 512 characters' },
        ]}
      >
        <Input placeholder="Enter location of the animal" />
      </Form.Item>

      <Form.Item
        label="Description"
        name="description"
        rules={[
          { required: true, message: 'Please enter description' },
          { max: 2000, message: 'Description cannot exceed 2000 characters' },
        ]}
      >
        <TextArea
          rows={4}
          placeholder="Describe the situation and animal condition"
          showCount
          maxLength={2000}
        />
      </Form.Item>

      <Form.Item label="Picture">
        <Upload
          listType="picture-card"
          fileList={fileList}
          beforeUpload={handleImageUpload}
          onRemove={handleImageRemove}
          maxCount={1}
          accept="image/*"
        >
          {fileList.length === 0 && (
            <div>
              <UploadOutlined />
              <div style={{ marginTop: 8 }}>Upload Image</div>
            </div>
          )}
        </Upload>
        <div style={{ color: '#888', fontSize: '12px', marginTop: '8px' }}>
          Optional. Max file size: 5MB. Supported formats: JPG, PNG, GIF, etc.
        </div>
      </Form.Item>

      <Form.Item
        label="Contact Number"
        name="contactNo"
        rules={[
          { required: true, message: 'Please enter contact number' },
          { max: 20, message: 'Contact number cannot exceed 20 characters' },
        ]}
      >
        <Input placeholder="Enter contact number" />
      </Form.Item>

      <Form.Item
        label="Contact Name"
        name="contactName"
        rules={[{ max: 100, message: 'Contact name cannot exceed 100 characters' }]}
      >
        <Input placeholder="Enter contact name (optional)" />
      </Form.Item>

      <Form.Item label="Active" name="isActive" valuePropName="checked">
        <Switch checkedChildren="Active" unCheckedChildren="Inactive" />
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

export default RequestRescueForm;
