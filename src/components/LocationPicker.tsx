import React, { useState, useEffect } from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMapEvents } from 'react-leaflet';
import { LatLng, Icon } from 'leaflet';
import { Button, message, Space, Input, Card } from 'antd';
import { EnvironmentOutlined, AimOutlined } from '@ant-design/icons';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';

const markerIcon = new URL('leaflet/dist/images/marker-icon.png', import.meta.url).href;
const markerIcon2x = new URL('leaflet/dist/images/marker-icon-2x.png', import.meta.url).href;
const markerShadow = new URL('leaflet/dist/images/marker-shadow.png', import.meta.url).href;

const DefaultIcon = L.icon({
  iconUrl: markerIcon,
  iconRetinaUrl: markerIcon2x,
  shadowUrl: markerShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
});

L.Marker.prototype.options.icon = DefaultIcon;

delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl: markerIcon2x,
  iconUrl: markerIcon,
  shadowUrl: markerShadow,
});

const DEFAULT_POSITION: [number, number] = [28.6139, 77.209];

interface LocationPickerProps {
  initialPosition?: [number, number];
  onLocationSelect: (lat: number, lng: number, address?: string) => void;
  disabled?: boolean;
  height?: number;
}

const LocationMarker: React.FC<{
  position: LatLng | null;
  setPosition: (pos: LatLng) => void;
  disabled?: boolean;
  onSelect: (pos: LatLng) => void;
}> = ({ position, setPosition, disabled, onSelect }) => {
  useMapEvents({
    click(e) {
      if (!disabled) {
        setPosition(e.latlng);
        onSelect(e.latlng);
        message.success('Location updated!');
      }
    },
  });

  return position === null ? null : (
    <Marker position={position}>
      <Popup>
        <strong>Selected Location</strong>
        <br />
        Lat: {position.lat.toFixed(6)}
        <br />
        Lng: {position.lng.toFixed(6)}
      </Popup>
    </Marker>
  );
};

const LocationPicker: React.FC<LocationPickerProps> = ({
  initialPosition = DEFAULT_POSITION,
  onLocationSelect,
  disabled = false,
  height = 400,
}) => {
  const [position, setPosition] = useState<LatLng | null>(null);
  const [address, setAddress] = useState<string>('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (initialPosition) {
      setPosition(new LatLng(initialPosition[0], initialPosition[1]));
    }
  }, [initialPosition]);

  const pushSelection = (lat: number, lng: number, addr?: string) => {
    onLocationSelect(lat, lng, addr);
  };

  const handleGetCurrentLocation = () => {
    if (disabled) {
      message.warning('Location picker is disabled');
      return;
    }

    if (!navigator.geolocation) {
      message.error('Geolocation is not supported by your browser');
      return;
    }

    setLoading(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        const { latitude, longitude } = pos.coords;
        const next = new LatLng(latitude, longitude);
        setPosition(next);
        pushSelection(latitude, longitude);
        message.success('Current location detected!');
        setLoading(false);
      },
      (error) => {
        console.error('Geolocation error:', error);
        message.error('Could not get your location. Please ensure location permissions are enabled.');
        setLoading(false);
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 0,
      }
    );
  };

  const handleConfirm = () => {
    if (!position) {
      message.warning('Please select a location on the map');
      return;
    }
    pushSelection(position.lat, position.lng, address || undefined);
    message.success('Location confirmed successfully!');
  };

  const handleSearch = async () => {
    if (!address.trim()) {
      message.warning('Please enter an address to search');
      return;
    }

    setLoading(true);
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&limit=1`
      );
      const data = await response.json();

      if (data && data.length > 0) {
        const { lat, lon } = data[0];
        const next = new LatLng(parseFloat(lat), parseFloat(lon));
        setPosition(next);
        pushSelection(next.lat, next.lng, address);
        message.success('Location found!');
      } else {
        message.error('Location not found. Please try a different address.');
      }
    } catch (error) {
      console.error('Geocoding error:', error);
      message.error('Error searching for location');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        <Space.Compact style={{ width: '100%' }}>
          <Input
            placeholder="Search for an address..."
            value={address}
            onChange={(e) => setAddress(e.target.value)}
            onPressEnter={handleSearch}
            prefix={<EnvironmentOutlined />}
            disabled={disabled || loading}
          />
          <Button onClick={handleSearch} loading={loading} disabled={disabled}>
            Search
          </Button>
        </Space.Compact>

        <Space wrap>
          <Button
            icon={<AimOutlined />}
            onClick={handleGetCurrentLocation}
            loading={loading}
            disabled={disabled}
          >
            Use Current Location
          </Button>
          <Button
            type="primary"
            onClick={handleConfirm}
            disabled={!position || disabled}
          >
            Confirm Location
          </Button>
          {position && (
            <span style={{ color: '#666', fontSize: '12px' }}>
              📍 {position.lat.toFixed(6)}, {position.lng.toFixed(6)}
            </span>
          )}
        </Space>

        <div
          style={{
            border: '1px solid #d9d9d9',
            borderRadius: '4px',
            overflow: 'hidden',
            opacity: disabled ? 0.6 : 1,
            pointerEvents: disabled ? 'none' : 'auto',
          }}
        >
          <MapContainer
            center={initialPosition}
            zoom={13}
            style={{ height: `${height}px`, width: '100%' }}
            scrollWheelZoom
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            <LocationMarker
              position={position}
              setPosition={setPosition}
              disabled={disabled}
              onSelect={(latlng) => pushSelection(latlng.lat, latlng.lng)}
            />
          </MapContainer>
        </div>

        <div style={{ fontSize: '12px', color: '#999' }}>
          💡 Click on the map to select a location, or use the buttons above
        </div>
      </Space>
    </Card>
  );
};

export default LocationPicker;