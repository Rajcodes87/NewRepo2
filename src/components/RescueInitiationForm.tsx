import React, { useEffect, useState } from 'react';
import { Form, Input, Button, Space, message, Select } from 'antd';
import {
  CreateRescueInitiationDto,
  UpdateRescueInitiationDto,
  RescueInitiationDto,
} from '../types/rescueInitiation';
import { RequestRescueDto } from '../types/requestRescue';
import RescueInitiationService from '../services/rescueInitiationService';
import RequestRescueService from '../services/requestRescueService';

const { TextArea } = Input;
const { Option } = Select;

interface RescueInitiationFormProps {
  editingRecord?: RescueInitiationDto;
  onSubmit: () => void;
  onCancel: () => void;
}

const RescueInitiationForm: React.FC<RescueInitiationFormProps> = ({
  editingRecord,
  onSubmit,
  onCancel,
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [requests, setRequests] = useState<RequestRescueDto[]>([]);
  const [loadingRequests, setLoadingRequests] = useState(false);

  useEffect(() => {
    // Load available rescue requests if creating new initiation
    if (!editingRecord) {
      fetchAvailableRequests();
    }

    if (editingRecord) {
      form.setFieldsValue({
        notes: editingRecord.notes,
      });
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
          status: 'NotInitiated',
        }
      );
      setRequests(result.items);
    } catch (error: any) {
      message.error(error.message || 'Failed to fetch rescue requests');
    } finally {
      setLoadingRequests(false);
    }
  };

  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      if (editingRecord) {
        const updateDto: UpdateRescueInitiationDto = {
          notes: values.notes,
        };
        await RescueInitiationService.update(editingRecord.id, updateDto);
        message.success('Rescue initiation updated successfully');
      } else {
        const createDto: CreateRescueInitiationDto = {
          requestRescueId: values.requestRescueId,
          notes: values.notes,
        };
        await RescueInitiationService.create(createDto);
        message.success('Rescue initiation created successfully');
      }
      form.resetFields();
      onSubmit();
    } catch (error: any) {
      message.error(error.message || 'Failed to save rescue initiation');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Form form={form} layout="vertical" onFinish={handleSubmit}>
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
        label="Notes"
        name="notes"
        rules={[{ max: 1000, message: 'Notes cannot exceed 1000 characters' }]}
      >
        <TextArea
          rows={4}
          placeholder="Add any notes or comments about your initiation"
          showCount
          maxLength={1000}
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

export default RescueInitiationForm;
