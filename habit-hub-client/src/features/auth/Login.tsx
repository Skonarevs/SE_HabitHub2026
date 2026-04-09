import { useForm } from 'react-hook-form';
import api from '../../api/axiosInstance';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAuthStore } from '../../store/useAuthStore';
import { Button } from '../../components/ui/Button';
import { useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';

interface LoginInputs {
  email: string;
  password: string;
  userType: 'Creator' | 'Member';
}

export const Login: React.FC = () => {
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<LoginInputs>({
    defaultValues: {
      userType: 'Member',
    },
  });

  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);

  const onSubmit = async (data: LoginInputs) => {
    try {
      const response = await api.post('/auth/login', data);
      login(
        response.data.name,
        response.data.sessionId,
        response.data.userType
      );
      if (response.data.userType === 'creator') {
        navigate('/main-creator', { replace: true });
      } else {
        navigate('/main-member', { replace: true });
      }
    } catch (error: any) {
      if (!error.response) {
        toast.error('Server is not responding. Please try again later.');
        return;
      }

      const status = Number(error.response.status);
      const serverMessage =
        error.response.data?.error || 'Something went wrong';

      if (status === 401 || status === 404) {
        setError('email', { type: 'server', message: serverMessage });
        setError('password', { type: 'server', message: serverMessage });
      } else if (status === 500) {
        toast.error('Server error. Please contact support.');
      } else {
        toast.error(`Unexpected error (${status})`);
      }
    }
  };

  return (
    <div
      className="min-h-screen bg-cover bg-center relative flex items-center justify-center p-4"
      style={{ backgroundImage: "url('/bg.jpg')" }}
    >
      <div className="absolute inset-0 bg-black/10" />

      <div className="relative w-full flex items-center justify-between px-20">
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="bg-white/90 backdrop-blur-md max-w-md w-full rounded-2xl shadow-2xl p-12 flex flex-col space-y-5"
        >
          <h2 className="text-2xl font-bold text-gray-900 mb-2">
            Welcome to HabitHub!
          </h2>

          {/* Email */}
          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              Email <span className="text-red-500">*</span>
            </label>
            <input
              {...register('email', {
                required: 'Email is required',
                pattern: {
                  value: /^\S+@\S+$/i,
                  message: 'Incorrect email format',
                },
              })}
              placeholder="mail@example.com"
              className={`w-full px-4 py-2.5 border rounded-lg outline-none transition-colors bg-gray-50 focus:bg-white ${
                errors.email
                  ? 'border-red-500 focus:ring-2 focus:ring-red-200'
                  : 'border-gray-200 focus:ring-2 focus:ring-gray-900 focus:border-gray-900'
              }`}
            />
            {errors.email && (
              <p className="text-sm text-red-500 mt-1">
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Password */}
          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              Password <span className="text-red-500">*</span>
            </label>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                {...register('password', { required: 'Password is required' })}
                placeholder="Enter password"
                className={`w-full px-4 py-2.5 pr-10 border rounded-lg outline-none transition-colors bg-gray-50 focus:bg-white ${
                  errors.password
                    ? 'border-red-500 focus:ring-2 focus:ring-red-200'
                    : 'border-gray-200 focus:ring-2 focus:ring-gray-900 focus:border-gray-900'
                }`}
              />
              <button
                type="button"
                onClick={() => setShowPassword((p) => !p)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.password && (
              <p className="text-sm text-red-500 mt-1">
                {errors.password.message}
              </p>
            )}
          </div>

          {/* User type */}
          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              I am logging in as
            </label>
            <select
              {...register('userType', { required: 'Select a role' })}
              className={`w-full px-4 py-2.5 border rounded-lg outline-none transition-colors bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-gray-900 ${
                errors.userType ? 'border-red-500' : 'border-gray-200'
              }`}
            >
              <option value="Member">Member</option>
              <option value="Creator">Creator</option>
            </select>
            {errors.userType && (
              <p className="text-sm text-red-500 mt-1">
                {errors.userType.message}
              </p>
            )}
          </div>

          {/* Submit */}
          <Button
            type="submit"
            isLoading={isSubmitting}
            className="w-full bg-gray-900 hover:bg-gray-800 text-white py-2.5 rounded-lg font-semibold tracking-widest uppercase text-sm transition"
          >
            Login
          </Button>

          <p className="text-center text-sm text-gray-600">
            Don't have an account?{' '}
            <Link
              to="/register"
              className="text-gray-900 font-semibold border-b border-gray-900 hover:opacity-70 transition-opacity"
            >
              Sign up
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
};
