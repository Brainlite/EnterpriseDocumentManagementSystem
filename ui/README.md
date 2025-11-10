# Enterprise Document Management System - UI

Modern React + TypeScript frontend for the Enterprise Document Management System with Material-UI components, role-based access control, and comprehensive document management features.

## Features

- **Document Management**: Upload, view, download, update, and delete documents
- **Advanced Search**: Search documents by title, description, tags, and filters
- **Tag Management**: Create and manage document tags
- **Document Sharing**: Share documents with specific users
- **Access Control**: Public, Private, and Restricted document access levels
- **Role-Based UI**: Different features based on user role (Viewer, Contributor, Manager, Admin)
- **Authentication**: JWT-based authentication with persistent sessions
- **Responsive Design**: Mobile-friendly Material-UI interface
- **Real-time Updates**: React Query for efficient data fetching and caching
- **Form Validation**: Formik + Yup for robust form handling

## Screenshots

### Login Page
![Login Page](../EnterpriseDocumentManagementSystem.Api/docs/1-login.png)

### My Documents - Dashboard
![My Documents Dashboard](../EnterpriseDocumentManagementSystem.Api/docs/2.1-my-docs.png)

### Document Loading State
![Document Loading](../EnterpriseDocumentManagementSystem.Api/docs/2-my-docs-loading.png)

### Search in Progress
![Search in Progress](../EnterpriseDocumentManagementSystem.Api/docs/3-my-docs-search-in-progress.png)

### Search Results
![Search Results](../EnterpriseDocumentManagementSystem.Api/docs/4-my-docs-search-result.png)

### Delete Document
![Delete Document](../EnterpriseDocumentManagementSystem.Api/docs/5-delete.png)

### Shared With Me
![Shared With Me](../EnterpriseDocumentManagementSystem.Api/docs/6-shared-with-me.png)

### Public Documents
![Public Documents](../EnterpriseDocumentManagementSystem.Api/docs/7-public.png)

### Upload Document
![Upload Document](../EnterpriseDocumentManagementSystem.Api/docs/8-add.png)

### Edit Document
![Edit Document](../EnterpriseDocumentManagementSystem.Api/docs/9-edit.png)

## Technology Stack

- **Framework**: React 19.1 with TypeScript
- **UI Library**: Material-UI (MUI) 7.3
- **Routing**: React Router DOM 7.9
- **State Management**: React Query (TanStack Query) 5.90
- **Forms**: Formik 2.4 + Yup 1.7
- **HTTP Client**: Axios 1.13
- **Build Tool**: Vite 7.1
- **Data Grid**: MUI X Data Grid 8.17

## Prerequisites

- Node.js 18.x or later
- npm or yarn
- Running API server (see API README)

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Environment Configuration

Create a `.env` file in the root of the `ui` folder:

```bash
VITE_API_URL=https://localhost:7052
```

For production, update this to your production API URL.

**Environment Files:**
- `.env` - Local development configuration (gitignored)
- `.env.example` - Example configuration file (committed to git)

### 3. Start Development Server

```bash
npm run dev
```

The application will be available at `http://localhost:5173`

### 4. Build for Production

```bash
npm run build
```

The production build will be in the `dist` folder.

### 5. Preview Production Build

```bash
npm run preview
```

## Project Structure

```
ui/
├── src/
│   ├── components/          # Reusable UI components
│   │   ├── DocumentCard.tsx
│   │   ├── DocumentGrid.tsx
│   │   ├── DocumentList.tsx
│   │   ├── SearchBar.tsx
│   │   ├── ShareDialog.tsx
│   │   ├── TagChip.tsx
│   │   └── UploadModal.tsx
│   ├── config/              # Configuration files
│   │   └── api.config.ts    # API URL configuration
│   ├── constants/           # Application constants
│   │   └── roles.ts         # User roles and permissions
│   ├── contexts/            # React contexts
│   │   └── AuthContext.tsx  # Authentication context
│   ├── hooks/               # Custom React hooks
│   │   └── useDocuments.ts  # Document management hook
│   ├── pages/               # Page components
│   │   ├── Dashboard.tsx    # Main dashboard
│   │   ├── LoginPage.tsx    # Login page
│   │   └── SharedDocuments.tsx
│   ├── services/            # API services
│   │   ├── api.ts           # Axios instance
│   │   ├── authService.ts   # Authentication API
│   │   └── documentService.ts # Document API
│   ├── types/               # TypeScript type definitions
│   │   └── index.ts
│   ├── App.tsx              # Root component
│   ├── main.tsx             # Application entry point
│   └── index.css            # Global styles
├── public/                  # Static assets
├── .env.example             # Example environment variables
├── package.json             # Dependencies and scripts
├── tsconfig.json            # TypeScript configuration
├── vite.config.ts           # Vite configuration
└── README.md                # This file
```

## Available Scripts

- `npm run dev` - Start development server with hot reload
- `npm run build` - Build for production
- `npm run preview` - Preview production build locally
- `npm run lint` - Run ESLint to check code quality

## User Roles and Permissions

### Viewer
- View public documents
- View documents shared with them
- Download accessible documents
- Search and filter documents

### Contributor
- All Viewer permissions
- Upload new documents
- Edit own documents
- Delete own documents
- Share own documents
- Create and manage tags

### Manager
- All Contributor permissions
- Manage team documents
- View team activity

### Admin
- All Manager permissions
- Manage all documents
- Manage users
- System administration

## Authentication

The application uses JWT token authentication:

1. Login with credentials
2. Token is stored in localStorage
3. Token is automatically included in API requests
4. Token is validated on protected routes
5. Automatic logout on token expiration

### Demo Users

- **Admin**: `admin@example.com` / `admin123`
- **Manager**: `manager@example.com` / `manager123`
- **Contributor**: `contributor@example.com` / `contributor123`
- **Viewer**: `viewer@example.com` / `viewer123`

## Key Features

### Document Upload
- Drag-and-drop or click to upload
- File type validation
- Size limit enforcement (10MB default)
- Metadata input (title, description, tags)
- Access level selection

### Document Search
- Full-text search
- Filter by tags
- Filter by access type
- Filter by owner
- Sort by date, title, or size

### Document Sharing
- Share with specific users
- Set permissions (View, Edit, Delete)
- View who has access
- Revoke access

### Tag Management
- Create custom tags
- Assign multiple tags to documents
- Filter by tags
- View popular tags

## API Integration

The UI communicates with the backend API using Axios. All API calls are centralized in the `services` folder:

- **authService.ts**: Authentication endpoints
- **documentService.ts**: Document management endpoints
- **api.ts**: Axios instance with interceptors

### Request Interceptors
- Automatically adds JWT token to requests
- Handles token refresh (if implemented)

### Response Interceptors
- Handles 401 Unauthorized (redirects to login)
- Handles network errors
- Formats error messages

## State Management

The application uses React Query for server state management:

- **Caching**: Automatic caching of API responses
- **Refetching**: Smart refetching on window focus
- **Mutations**: Optimistic updates for better UX
- **Invalidation**: Automatic cache invalidation after mutations

## Styling

The application uses Material-UI with custom theming:

- **Theme**: Customized MUI theme
- **Responsive**: Mobile-first design
- **Dark Mode**: (Optional) Can be implemented
- **Custom Components**: Styled components for specific needs

## Development Guidelines

### Code Style
- Use TypeScript for type safety
- Follow React best practices
- Use functional components with hooks
- Keep components small and focused
- Use meaningful variable and function names

### Component Structure
```typescript
// 1. Imports
import React from 'react';
import { Component } from '@mui/material';

// 2. Types/Interfaces
interface Props {
  // ...
}

// 3. Component
export const MyComponent: React.FC<Props> = ({ prop }) => {
  // 4. Hooks
  const [state, setState] = useState();
  
  // 5. Effects
  useEffect(() => {
    // ...
  }, []);
  
  // 6. Handlers
  const handleClick = () => {
    // ...
  };
  
  // 7. Render
  return (
    <div>
      {/* ... */}
    </div>
  );
};
```

### Error Handling
- Use try-catch for async operations
- Display user-friendly error messages
- Log errors for debugging
- Provide fallback UI for errors

## Troubleshooting

### API Connection Issues
- Verify API is running at the configured URL
- Check CORS settings in API
- Verify `.env` file exists and has correct URL

### Authentication Issues
- Clear localStorage and try logging in again
- Check token expiration
- Verify API JWT configuration matches

### Build Issues
- Delete `node_modules` and run `npm install` again
- Clear Vite cache: `rm -rf node_modules/.vite`
- Check for TypeScript errors: `npm run build`

## Performance Optimization

- **Code Splitting**: Automatic route-based code splitting
- **Lazy Loading**: Components loaded on demand
- **Memoization**: Use React.memo for expensive components
- **Virtual Scrolling**: For large document lists (MUI Data Grid)
- **Image Optimization**: Lazy load document thumbnails

## Security Considerations

1. **XSS Prevention**: React automatically escapes content
2. **CSRF Protection**: Token-based authentication
3. **Secure Storage**: Tokens stored in localStorage (consider httpOnly cookies for production)
4. **Input Validation**: Client-side validation with Yup
5. **HTTPS**: Always use HTTPS in production

## Deployment

### Build for Production
```bash
npm run build
```

### Deploy to Static Hosting
The `dist` folder can be deployed to:
- Netlify
- Vercel
- AWS S3 + CloudFront
- Azure Static Web Apps
- GitHub Pages

### Environment Variables for Production
Set the following environment variable in your hosting platform:
```
VITE_API_URL=https://your-production-api.com
```

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)


