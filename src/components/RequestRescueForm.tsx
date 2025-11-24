import React, { useState, useEffect } from 'react';
import { Form, Input, Button, Select, Upload, message, Row, Col, Divider } from 'antd';
import { UploadOutlined, EnvironmentOutlined } from '@ant-design/icons';
import type { UploadFile, RcFile } from 'antd/es/upload/interface';
import { CreateUpdateRequestRescue, RequestRescueDto } from '../types/requestRescue';
import LocationPicker from './LocationPicker';
import RequestRescueService from '../services/requestRescueService';

const { TextArea } = Input;
const { Option } = Select;

interface RequestRescueFormProps {
  editingRecord?: RequestRescueDto; // ✅ FIXED: Changed from initialValues
  onSubmit: () => void; // ✅ FIXED: Callback after successful submission
  onCancel: () => void; // ✅ ADDED: Cancel callback
}

const RequestRescueForm: React.FC<RequestRescueFormProps> = ({
  editingRecord,
  onSubmit,
  onCancel,
}) => {
  const [form] = Form.useForm();
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [base64Image, setBase64Image] = useState<string>('');
  const [showLocationPicker, setShowLocationPicker] = useState(false);
  const [loading, setLoading] = useState(false); // ✅ ADDED: Local loading state
  const [locationData, setLocationData] = useState<{
    latitude?: number;
    longitude?: number;
  }>({
    latitude: editingRecord?.latitude,
    longitude: editingRecord?.longitude,
  });

  useEffect(() => {
    if (editingRecord) {
      form.setFieldsValue({
        title: editingRecord.title,
        location: editingRecord.location,
        description: editingRecord.description,
        contactNo: editingRecord.contactNo,
        contactName: editingRecord.contactName,
        severity: editingRecord.severity,
      });

      if (editingRecord.picture) {
        setBase64Image(editingRecord.picture);
        setFileList([
          {
            uid: '-1',
            name: 'image.png',
            status: 'done',
            url: editingRecord.picture,
          },
        ]);
      }

      if (editingRecord.latitude && editingRecord.longitude) {
        setLocationData({
          latitude: editingRecord.latitude,
          longitude: editingRecord.longitude,
        });
        setShowLocationPicker(true);
      }
    }
  }, [editingRecord, form]);

  const getBase64 = (file: RcFile): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = (error) => reject(error);
    });
  };

  const handleUploadChange = async (info: any) => {
    let newFileList = [...info.fileList];
    newFileList = newFileList.slice(-1);
    setFileList(newFileList);

    if (newFileList.length > 0 && newFileList[0].originFileObj) {
      try {
        const base64 = await getBase64(newFileList[0].originFileObj as RcFile);
        setBase64Image(base64);
      } catch (error) {
        message.error('Error reading file');
      }
    } else {
      setBase64Image('');
    }
  };

  const beforeUpload = (file: RcFile) => {
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

    return false;
  };

  const handleLocationSelect = (lat: number, lng: number, address?: string) => {
    setLocationData({ latitude: lat, longitude: lng });
    form.setFieldsValue({
      latitude: lat,
      longitude: lng,
      mapUrl: `https://www.openstreetmap.org/?mlat=${lat}&mlon=${lng}#map=15/${lat}/${lng}`,
    });
    message.success('Location selected successfully!');
  };

  // ✅ FIXED: Actual submission logic
  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      const submitData: CreateUpdateRequestRescue = {
        title: values.title,
        location: values.location,
        description: values.description,
        contactNo: values.contactNo,
        contactName: values.contactName,
        severity: values.severity,
        picture: base64Image || undefined,
        latitude: locationData.latitude,
        longitude: locationData.longitude,
        mapUrl: locationData.latitude && locationData.longitude
          ? `https://www.openstreetmap.org/?mlat=${locationData.latitude}&mlon=${locationData.longitude}#map=15/${locationData.latitude}/${locationData.longitude}`
          : undefined,
      };

      if (editingRecord) {
        // Update existing
        await RequestRescueService.update(editingRecord.id, submitData);
        message.success('Rescue request updated successfully!');
      } else {
        // Create new
        await RequestRescueService.create(submitData);
        message.success('Rescue request created successfully!');
      }

      form.resetFields();
      setFileList([]);
      setBase64Image('');
      setLocationData({});
      onSubmit(); // Call parent callback
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
      initialValues={{
        severity: 'Medium',
      }}
    >
      <Row gutter={16}>
        <Col span={24}>
          <Form.Item
            label="Title"
            name="title"
            rules={[
              { required: true, message: 'Please enter a title' },
              { max: 200, message: 'Title must be less than 200 characters' },
            ]}
          >
            <Input placeholder="e.g., Injured dog needs immediate help" />
          </Form.Item>
        </Col>

        <Col span={12}>
          <Form.Item
            label="Location (Address)"
            name="location"
            rules={[
              { required: true, message: 'Please enter a location' },
              { max: 500, message: 'Location must be less than 500 characters' },
            ]}
          >
            <Input
              prefix={<EnvironmentOutlined />}
              placeholder="Street, City, State"
            />
          </Form.Item>
        </Col>

        <Col span={12}>
          <Form.Item
            label="Severity"
            name="severity"
            rules={[{ required: true, message: 'Please select severity' }]}
          >
            <Select>
              <Option value="Low">🟢 Low</Option>
              <Option value="Medium">🟡 Medium</Option>
              <Option value="High">🟠 High</Option>
              <Option value="Critical">🔴 Critical</Option>
            </Select>
          </Form.Item>
        </Col>

        <Col span={24}>
          <Form.Item
            label="Description"
            name="description"
            rules={[
              { required: true, message: 'Please enter a description' },
              { max: 2000, message: 'Description must be less than 2000 characters' },
            ]}
          >
            <TextArea
              rows={4}
              placeholder="Provide detailed information about the animal and situation..."
            />
          </Form.Item>
        </Col>

        <Col span={12}>
          <Form.Item
            label="Contact Number"
            name="contactNo"
            rules={[
              { required: true, message: 'Please enter a contact number' },
              { pattern: /^[0-9+\-() ]{7,20}$/, message: 'Please enter a valid phone number' },
            ]}
          >
            <Input placeholder="+1234567890" />
          </Form.Item>
        </Col>

        <Col span={12}>
          <Form.Item
            label="Contact Name"
            name="contactName"
            rules={[{ max: 100, message: 'Name must be less than 100 characters' }]}
          >
            <Input placeholder="Your name (optional)" />
          </Form.Item>
        </Col>

        <Col span={24}>
          <Form.Item label="Picture">
            <Upload
              listType="picture-card"
              fileList={fileList}
              onChange={handleUploadChange}
              beforeUpload={beforeUpload}
              accept="image/*"
              maxCount={1}
            >
              {fileList.length === 0 && (
                <div>
                  <UploadOutlined />
                  <div style={{ marginTop: 8 }}>Upload Photo</div>
                </div>
              )}
            </Upload>
            {base64Image && (
              <div style={{ marginTop: 8 }}>
                <img
                  src={base64Image}
                  alt="Preview"
                  style={{ maxWidth: '200px', maxHeight: '200px', objectFit: 'cover' }}
                />
              </div>
            )}
          </Form.Item>
        </Col>
      </Row>

      <Divider orientation="horizontal">📍 GPS Location (Recommended)</Divider>
      
      <div style={{ marginBottom: 16 }}>
        <Button
          type={showLocationPicker ? 'default' : 'dashed'}
          onClick={() => setShowLocationPicker(!showLocationPicker)}
          block
        >
          {showLocationPicker ? '📍 Hide Location Picker' : '📍 Add GPS Location (Helps rescuers find you faster!)'}
        </Button>
        {locationData.latitude && locationData.longitude && (
          <div style={{ marginTop: 8, padding: 8, background: '#f0f0f0', borderRadius: 4, fontSize: 12 }}>
            ✅ Location set: {locationData.latitude.toFixed(6)}, {locationData.longitude.toFixed(6)}
          </div>
        )}
      </div>

      {showLocationPicker && (
        <div style={{ marginBottom: 24 }}>
          <LocationPicker
            initialPosition={
              locationData.latitude && locationData.longitude
                ? [locationData.latitude, locationData.longitude]
                : undefined
            }
            onLocationSelect={handleLocationSelect}
            height={350}
          />
        </div>
      )}

      <Form.Item name="latitude" hidden>
        <Input />
      </Form.Item>
      <Form.Item name="longitude" hidden>
        <Input />
      </Form.Item>
      <Form.Item name="mapUrl" hidden>
        <Input />
      </Form.Item>

      <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading} size="large">
            {editingRecord ? 'Update Rescue Request' : 'Submit Rescue Request'}
          </Button>
          <Button onClick={onCancel} size="large">
            Cancel
          </Button>
      </Form.Item>
    </Form>
  );
};

export default RequestRescueForm;