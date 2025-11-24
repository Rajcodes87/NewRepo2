# Frontend Integration Guide - Rescuer Authentication & Authorization

## Overview
This guide explains how to integrate the Rescuer authentication and authorization features in your frontend application.

## Backend Changes Summary

### 1. **Rescuer Role Created**
   - A new "Rescuer" role has been created via `RescuerRoleDataSeeder`
   - This role is automatically seeded when you run database migrations

### 2. **Public Registration Endpoint**
   - **Endpoint**: `POST /api/app/rescuer-registration/register`
   - **Access**: Public (no authentication required)
   - **Purpose**: Allow users to sign up as Rescuers

### 3. **Authorization Rules**
   - **RequestRescue**: Public access (anyone can view and create rescue requests)
   - **RescueInitiation**: Only Rescuers and Admins
   - **RescueCompletion**: Only Rescuers and Admins

---

## API Endpoints

### 1. Rescuer Registration (Public)

**Endpoint**: `POST https://localhost:44365/api/app/rescuer-registration/register`

**Request Body**:
```json
{
  "userName": "johndoe",
  "email": "john@example.com",
  "password": "YourPassword123!",
  "name": "John",
  "surname": "Doe",
  "phoneNumber": "+1234567890"
}
```

**Response** (Success - 200):
```json
{
  "code": 200,
  "success": true,
  "message": "Registration successful. You can now log in as a Rescuer.",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "johndoe",
    "email": "john@example.com"
  }
}
```

**Response** (Error - 400):
```json
{
  "code": 400,
  "success": false,
  "message": "Username is already taken.",
  "data": null
}
```

---

### 2. Admin Login (Existing OpenIddict Flow)

**Endpoint**: `POST https://localhost:44365/connect/token`

**Request Body** (x-www-form-urlencoded):
```
grant_type: password
client_id: Pawchums_App
username: admin
password: 1q2w3E*
scope: offline_access Pawchums
```

**Response**:
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "...",
  "scope": "offline_access Pawchums"
}
```

---

### 3. Rescuer Login (Same as Admin, different credentials)

Use the same endpoint as Admin login, but with Rescuer credentials:

**Request Body**:
```
grant_type: password
client_id: Pawchums_App
username: johndoe
password: YourPassword123!
scope: offline_access Pawchums
```

---

## Frontend Implementation Steps

### Step 1: Update Your Login UI

Create two separate login flows:

```jsx
// Example React components

// Admin Login Button
<Button onClick={handleAdminLogin}>
  Login as Admin
</Button>

// Rescuer Login/Signup Section
<div>
  <Button onClick={handleRescuerLogin}>
    Login as Rescuer
  </Button>
  <Button onClick={handleShowRescuerSignup}>
    Sign up as Rescuer
  </Button>
</div>
```

### Step 2: Implement Rescuer Registration

```javascript
async function registerRescuer(formData) {
  try {
    const response = await fetch('https://localhost:44365/api/app/rescuer-registration/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        userName: formData.username,
        email: formData.email,
        password: formData.password,
        name: formData.name,
        surname: formData.surname,
        phoneNumber: formData.phoneNumber
      })
    });

    const result = await response.json();

    if (result.success) {
      // Show success message and redirect to login
      alert('Registration successful! Please log in.');
      redirectToLogin();
    } else {
      // Show error message
      alert(result.message);
    }
  } catch (error) {
    console.error('Registration error:', error);
    alert('An error occurred during registration.');
  }
}
```

### Step 3: Implement Login Function

```javascript
async function login(username, password, isAdmin = false) {
  const formData = new URLSearchParams();
  formData.append('grant_type', 'password');
  formData.append('client_id', 'Pawchums_App');
  formData.append('username', username);
  formData.append('password', password);
  formData.append('scope', 'offline_access Pawchums');

  try {
    const response = await fetch('https://localhost:44365/connect/token', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: formData
    });

    const tokenData = await response.json();

    if (tokenData.access_token) {
      // Store token in localStorage or secure storage
      localStorage.setItem('access_token', tokenData.access_token);
      localStorage.setItem('refresh_token', tokenData.refresh_token);

      // Decode token to get user role
      const userRole = getUserRoleFromToken(tokenData.access_token);
      localStorage.setItem('user_role', userRole);

      // Redirect based on role
      if (userRole === 'admin') {
        window.location.href = '/admin/dashboard';
      } else if (userRole === 'Rescuer') {
        window.location.href = '/rescuer/dashboard';
      }
    } else {
      alert('Invalid credentials');
    }
  } catch (error) {
    console.error('Login error:', error);
    alert('An error occurred during login.');
  }
}

// Helper function to decode JWT and extract role
function getUserRoleFromToken(token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  return payload.role; // ABP stores role in 'role' claim
}
```

### Step 4: Implement Route Protection

```javascript
// Example route protection in React Router

import { Navigate } from 'react-router-dom';

function ProtectedRoute({ children, allowedRoles }) {
  const token = localStorage.getItem('access_token');
  const userRole = localStorage.getItem('user_role');

  if (!token) {
    return <Navigate to="/login" />;
  }

  if (allowedRoles && !allowedRoles.includes(userRole)) {
    return <Navigate to="/unauthorized" />;
  }

  return children;
}

// Usage in routes:
<Routes>
  {/* Public routes */}
  <Route path="/rescue-requests" element={<RescueRequestsPage />} />
  <Route path="/login" element={<LoginPage />} />
  <Route path="/signup-rescuer" element={<RescuerSignupPage />} />

  {/* Rescuer-only routes */}
  <Route path="/rescue-initiations" element={
    <ProtectedRoute allowedRoles={['Rescuer', 'admin']}>
      <RescueInitiationsPage />
    </ProtectedRoute>
  } />

  <Route path="/rescue-completions" element={
    <ProtectedRoute allowedRoles={['Rescuer', 'admin']}>
      <RescueCompletionsPage />
    </ProtectedRoute>
  } />

  {/* Admin-only routes */}
  <Route path="/admin/*" element={
    <ProtectedRoute allowedRoles={['admin']}>
      <AdminDashboard />
    </ProtectedRoute>
  } />
</Routes>
```

### Step 5: Add Authorization Headers to API Calls

```javascript
// Helper function to make authorized API calls
async function apiCall(endpoint, options = {}) {
  const token = localStorage.getItem('access_token');

  const headers = {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
    ...options.headers
  };

  const response = await fetch(`https://localhost:44365${endpoint}`, {
    ...options,
    headers
  });

  if (response.status === 401) {
    // Token expired, redirect to login
    localStorage.removeItem('access_token');
    window.location.href = '/login';
    return;
  }

  return response.json();
}

// Example usage:
async function getRescueInitiations() {
  return await apiCall('/api/app/rescue-initiation?skipCount=0&maxResultCount=10');
}
```

---

## Access Control Matrix

| Page/Feature | Anonymous | Rescuer | Admin |
|--------------|-----------|---------|-------|
| View Rescue Requests | ✅ | ✅ | ✅ |
| Create Rescue Request | ✅ | ✅ | ✅ |
| View Rescue Initiations | ❌ | ✅ | ✅ |
| Create Rescue Initiation | ❌ | ✅ | ✅ |
| View Rescue Completions | ❌ | ✅ | ✅ |
| Create Rescue Completion | ❌ | ✅ | ✅ |
| Admin Dashboard | ❌ | ❌ | ✅ |

---

## Testing the Implementation

### 1. Run Database Migrations

Before testing, ensure you've run the migrations to seed the Rescuer role:

```bash
cd src/Pawchums.DbMigrator
dotnet run
```

### 2. Test Rescuer Registration

```bash
curl -X POST https://localhost:44365/api/app/rescuer-registration/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testrescuer",
    "email": "testrescuer@example.com",
    "password": "Test123!",
    "name": "Test",
    "surname": "Rescuer",
    "phoneNumber": "+1234567890"
  }'
```

### 3. Test Rescuer Login

```bash
curl -X POST https://localhost:44365/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=Pawchums_App&username=testrescuer&password=Test123!&scope=offline_access Pawchums"
```

### 4. Test Protected Endpoint

```bash
# Should return 401 without token
curl -X GET https://localhost:44365/api/app/rescue-initiation

# Should work with token
curl -X GET https://localhost:44365/api/app/rescue-initiation \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

---

## Important Notes

1. **Anti-Forgery Token**: We've disabled automatic anti-forgery validation for APIs. If you need to enable it selectively, you'll need to handle CSRF tokens in your frontend.

2. **Email Confirmation**: The current implementation doesn't require email confirmation. You may want to implement this for production.

3. **Password Policy**: The default ABP password policy requires:
   - Minimum 6 characters
   - At least one uppercase letter
   - At least one lowercase letter
   - At least one digit
   - At least one special character

4. **Token Expiration**: Access tokens expire after 1 hour by default. Implement token refresh using the `refresh_token` for better UX.

5. **CORS**: Ensure your frontend URL is added to the `appsettings.json` CORS origins:
   ```json
   "App": {
     "CorsOrigins": "http://localhost:3000,http://localhost:3001,http://localhost:3002"
   }
   ```

---

## Next Steps

1. ✅ Run database migrations to seed the Rescuer role
2. ✅ Test the registration endpoint
3. ✅ Implement the frontend UI components
4. ✅ Test the complete flow (signup → login → access protected pages)
5. ⚠️ Consider adding email confirmation for production
6. ⚠️ Implement proper error handling and validation messages
7. ⚠️ Add loading states and better UX for form submissions

---

## Support

If you encounter any issues:
1. Check the backend logs: `src/Pawchums.HttpApi.Host/Logs/logs.txt`
2. Check browser console for frontend errors
3. Verify CORS configuration
4. Ensure the database migrations have been run

