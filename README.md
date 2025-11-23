# Pawchums - Animal Rescue System Frontend

A modern React + TypeScript frontend application for the Pawchums Animal Rescue System, built with Vite and Ant Design.

## Features

- **Rescue Requests Management**: Create, read, update, delete, and manage rescue requests
- **Rescue Initiations**: Handle rescuer interest and initiation workflow
- **Rescue Completions**: Track and verify completed rescue operations
- **Modern UI**: Built with Ant Design components for a professional look
- **TypeScript**: Full type safety throughout the application
- **Responsive Design**: Works seamlessly on desktop and mobile devices

## Technology Stack

- **React 19** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **Ant Design 6** - UI component library
- **React Router 7** - Client-side routing
- **Axios** - HTTP client
- **Day.js** - Date formatting

## Project Structure

```
src/
├── components/           # Reusable React components
│   ├── RequestRescueForm.tsx
│   ├── RescueInitiationForm.tsx
│   └── RescueCompletionForm.tsx
├── pages/               # Page components
│   ├── RequestRescueList.tsx
│   ├── RescueInitiationList.tsx
│   └── RescueCompletionList.tsx
├── services/            # API service layer
│   ├── requestRescueService.ts
│   ├── rescueInitiationService.ts
│   └── rescueCompletionService.ts
├── types/               # TypeScript type definitions
│   ├── common.ts
│   ├── requestRescue.ts
│   ├── rescueInitiation.ts
│   └── rescueCompletion.ts
├── config/              # Configuration files
│   └── httpClient.ts    # Axios configuration
├── App.tsx              # Main application component
└── main.tsx            # Application entry point
```

## Prerequisites

- Node.js (v18 or higher)
- npm or yarn
- Backend API running at `https://localhost:44365`

## Installation

1. Navigate to the project directory:
```bash
cd PawchumsUi
```

2. Install dependencies:
```bash
npm install
```

## Configuration

The frontend is configured to proxy API requests to the backend:

- **API Base URL**: `https://localhost:44365`
- **Auth Server**: `https://localhost:44363`
- **Frontend Port**: `3000`

Proxy configuration is in `vite.config.js`:
```javascript
proxy: {
  '/api': {
    target: 'https://localhost:44365',
    changeOrigin: true,
    secure: false,
  }
}
```

## Running the Application

### Development Mode

Start the development server with hot reload:
```bash
npm run dev
```

The application will be available at `http://localhost:3000`

### Production Build

Build the application for production:
```bash
npm run build
```

Build output will be in the `dist/` directory.

### Preview Production Build

Preview the production build locally:
```bash
npm run preview
```

## API Integration

The frontend integrates with three main backend services:

### 1. Request Rescue Service
- **Base URL**: `/api/app/request-rescue`
- **Operations**: Create, Read, Update, Delete, Activate, Deactivate
- **Features**: Search, filter by status and active state, pagination

### 2. Rescue Initiation Service
- **Base URL**: `/api/app/rescue-initiation`
- **Operations**: Create, Read, Update, Delete, Accept, Reject, Withdraw
- **Features**: Search, filter by status, view by request or rescuer

### 3. Rescue Completion Service
- **Base URL**: `/api/app/rescue-completion`
- **Operations**: Create, Read, Update, Delete, Verify, Unverify
- **Features**: Search, filter by verification status, date range filtering

## Key Features Implementation

### Rescue Requests
- View all rescue requests in a paginated table
- Create new rescue requests with title, location, description, contact info
- Edit existing requests (title, location, description, picture URL, contact)
- Delete requests (with validation - cannot delete if initiations exist)
- Activate/Deactivate requests
- Filter by status (NotInitiated, Initiated, InProgress, Completed, Cancelled)
- Search by keyword
- View initiation count and completion status

### Rescue Initiations
- View all rescue initiations
- Create initiation for available requests
- Update notes on pending initiations
- Accept initiations (admin) - automatically rejects others
- Reject initiations (admin)
- Withdraw initiations (rescuer)
- Delete initiations (only if not accepted)
- Filter by status (Pending, Accepted, Rejected, Withdrawn)
- View rescuer details

### Rescue Completions
- View all rescue completions
- Create completion records with proof and description
- Edit completion details (only unverified)
- Delete completions (only unverified)
- Verify completions (admin) with verification notes
- Unverify completions (admin)
- Filter by verification status
- Date range filtering

## Authentication

The application includes authentication token handling:
- Token stored in `localStorage` as `access_token`
- Automatically attached to all API requests
- Redirects to login on 401 Unauthorized

## Development Guidelines

### Adding New Features

1. Create TypeScript types in `src/types/`
2. Create service methods in `src/services/`
3. Create UI components in `src/components/`
4. Create page components in `src/pages/`
5. Add routes in `src/App.tsx`

### Code Style

- Use functional components with hooks
- Use TypeScript for type safety
- Follow Ant Design conventions
- Keep components focused and reusable
- Use async/await for API calls

## Troubleshooting

### CORS Issues
Ensure the backend CORS policy includes `http://localhost:3000` in allowed origins.

### API Connection Issues
Verify the backend is running at `https://localhost:44365` and accessible.

### Build Errors
Clear node_modules and reinstall:
```bash
rm -rf node_modules package-lock.json
npm install
```

## Future Enhancements

- Dashboard with statistics and charts
- Real-time notifications using SignalR
- Image upload functionality
- Map integration for location visualization
- Advanced filtering and sorting
- Export functionality (PDF, Excel)
- Mobile-responsive improvements
- Dark mode theme
- Multi-language support

## Contributing

When contributing to this project:
1. Follow the existing code structure
2. Add TypeScript types for new features
3. Write meaningful commit messages
4. Test all CRUD operations before committing

## License

Copyright © 2025 Pawchums - Animal Rescue System
