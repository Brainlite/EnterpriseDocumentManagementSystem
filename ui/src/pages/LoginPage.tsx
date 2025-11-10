import { useNavigate } from 'react-router-dom';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { useMutation } from '@tanstack/react-query';
import { authService } from '../services/authService';
import { useAuth } from '../contexts/AuthContext';
import type { LoginRequest } from '../types/auth';
import './LoginPage.css';

const loginSchema = Yup.object().shape({
  email: Yup.string()
    .email('Invalid email address')
    .required('Email is required'),
  password: Yup.string()
    .min(6, 'Password must be at least 6 characters')
    .required('Password is required'),
});

export const LoginPage = () => {
  const navigate = useNavigate();
  const { login } = useAuth();

  const loginMutation = useMutation({
    mutationFn: (credentials: LoginRequest) => authService.login(credentials),
    onSuccess: (data) => {
      login(data.token, data.user);
      navigate('/dashboard');
    },
  });

  const initialValues: LoginRequest = {
    email: '',
    password: '',
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h1>Enterprise Document Management</h1>
        <h2>Login</h2>
        
        <Formik
          initialValues={initialValues}
          validationSchema={loginSchema}
          onSubmit={(values, { setSubmitting }) => {
            loginMutation.mutate(values, {
              onSettled: () => {
                setSubmitting(false);
              },
            });
          }}
        >
          {({ isSubmitting }) => (
            <Form className="login-form">
              <div className="form-group">
                <label htmlFor="email">Email</label>
                <Field
                  type="email"
                  id="email"
                  name="email"
                  className="form-input"
                  placeholder="Enter your email"
                />
                <ErrorMessage name="email" component="div" className="error-message" />
              </div>

              <div className="form-group">
                <label htmlFor="password">Password</label>
                <Field
                  type="password"
                  id="password"
                  name="password"
                  className="form-input"
                  placeholder="Enter your password"
                />
                <ErrorMessage name="password" component="div" className="error-message" />
              </div>

              {loginMutation.isError && (
                <div className="error-message">
                  {loginMutation.error instanceof Error
                    ? loginMutation.error.message.includes('Network Error') || loginMutation.error.message.includes('CORS')
                      ? 'Cannot connect to server. Please ensure the backend API is running.'
                      : loginMutation.error.message
                    : 'Invalid email or password'}
                </div>
              )}

              <button
                type="submit"
                className="login-button"
                disabled={isSubmitting || loginMutation.isPending}
              >
                {loginMutation.isPending ? 'Logging in...' : 'Login'}
              </button>
            </Form>
          )}
        </Formik>
      </div>
    </div>
  );
};
