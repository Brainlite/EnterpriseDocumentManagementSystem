# Testing Guide - Enterprise Document Management System

Comprehensive testing setup for both API and UI with unit tests, mocking, and best practices.

## Overview

This project includes complete unit test coverage for both the backend API and frontend UI:

- **API Tests**: xUnit + Moq + FluentAssertions
- **UI Tests**: Vitest + React Testing Library + User Event

## Quick Start

### Run All Tests

```bash
# API tests
cd EnterpriseDocumentManagementSystem.Api.Tests
dotnet test

# UI tests
cd ui
npm test
```

## API Testing Summary

### Test Coverage
- **40+ unit tests** across services and controllers
- **DocumentAuthorizationService**: 13 tests
- **LocalFileStorageService**: 10 tests
- **MockAuthService**: 11 tests
- **AuthController**: 6 tests

### Running Tests
```bash
dotnet test                                    # Run all tests
dotnet test --collect:"XPlat Code Coverage"   # With coverage
dotnet watch test                              # Watch mode
```

## UI Testing Summary

### Test Coverage
- **26+ unit tests** across components and services
- **Component tests**: AccessTypeSelect, ProtectedRoute
- **Service tests**: authService, documentService

### Running Tests
```bash
npm test                # Run all tests
npm run test:ui         # Interactive UI
npm run test:coverage   # With coverage
```

## Documentation

- [API Test Documentation](EnterpriseDocumentManagementSystem.Api.Tests/README.md)
- [UI Test Documentation](ui/README.tests.md)

## Test Frameworks

### Backend (.NET)
- xUnit - Test framework
- Moq - Mocking library
- FluentAssertions - Assertion library

### Frontend (React)
- Vitest - Test runner
- React Testing Library - Component testing
- Testing Library User Event - User interactions
