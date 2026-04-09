import { useForm } from 'react-hook-form';
import api from '../../api/axiosInstance';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAuthStore } from '../../store/useAuthStore';
import { Button } from '../../components/ui/Button';
import { useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';

interface RegisterFormInputs {
  name: string;
  email: string;
  password: string;
  timezone: string;
  userType: 'Creator' | 'Member';
}

export const Register: React.FC = () => {
  const [showPassword, setShowPassword] = useState(false);
  const detectedTimezone =
    Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC';
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<RegisterFormInputs>({
    defaultValues: {
      timezone: detectedTimezone,
      userType: 'Member',
    },
  });

  const passwordValue = watch('password', '');
  const hasNumber = /\d/.test(passwordValue);
  const hasMinLength = passwordValue.length >= 8;

  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);

  const onSubmit = async (data: RegisterFormInputs) => {
    try {
      const response = await api.post('/auth/register', data);
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

      switch (status) {
        case 409:
          setError('email', { type: 'server', message: serverMessage });
          break;
        case 400:
          toast.error(serverMessage);
          break;
        case 500:
          toast.error('Server error. Please contact support.');
          break;
        default:
          toast.error(`Unexpected error (${status})`);
      }
    }
  };

  return (
    <div
      className="min-h-screen bg-cover bg-center relative flex items-center justify-center p-4"
      style={{ backgroundImage: "url('/bg.jpg')" }}
      //   style = {{backgroundColor: 'black'}}
    >
      <div className="absolute inset-0 bg-black/10" />

      <div className="relative w-full flex items-center justify-between px-20">
        <form
          onSubmit={handleSubmit(onSubmit, () => {
            toast.error('Please fix the errors before submitting.');
          })}
          className="bg-white/90 backdrop-blur-md max-w-md w-full rounded-2xl shadow-2xl p-12 flex flex-col space-y-5"
        >
          <h2 className="text-2xl font-bold text-gray-900 mb-2">
            Join HabitHub Now
          </h2>

          {/* Name */}
          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              Name <span className="text-red-500">*</span>
            </label>
            <input
              {...register('name', { required: 'Name is required' })}
              placeholder="Name"
              className={`w-full px-4 py-2.5 border rounded-lg outline-none transition-colors bg-gray-50 focus:bg-white ${
                errors.name
                  ? 'border-red-500 focus:ring-2 focus:ring-red-200'
                  : 'border-gray-200 focus:ring-2 focus:ring-gray-900 focus:border-gray-900'
              }`}
            />
            {errors.name && (
              <p className="text-sm text-red-500 mt-1">{errors.name.message}</p>
            )}
          </div>

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

          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              I want to...
            </label>
            <select
              {...register('userType', { required: 'Select a role' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-gray-900 focus:border-gray-900 bg-white"
            >
              <option value="Member">Join a team</option>
              <option value="Creator">Create my own team</option>
            </select>
          </div>

          {/* Password */}
          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              Password <span className="text-red-500">*</span>
            </label>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                {...register('password', {
                  required: 'Password is required',
                  validate: {
                    minLength: (v) => v.length >= 8 || 'Minimum 8 characters',
                    hasNumber: (v) =>
                      /\d/.test(v) || 'Must contain at least one number',
                  },
                })}
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

            {/* Password hints */}
            <div className="mt-2 space-y-1">
              <p className="text-xs text-gray-400 mb-1">
                Your password need to include:
              </p>
              {[
                { valid: hasNumber, label: 'Must contain one number' },
                { valid: hasMinLength, label: 'Min 8 characters' },
              ].map(({ valid, label }) => (
                <div key={label} className="flex items-center gap-2">
                  <span
                    className={`w-4 h-4 rounded-full flex items-center justify-center shrink-0 transition-colors ${
                      valid ? 'bg-gray-900' : 'border border-gray-300'
                    }`}
                  >
                    {valid && (
                      <svg width="9" height="7" viewBox="0 0 9 7" fill="none">
                        <path
                          d="M1 3.5L3.5 6L8 1"
                          stroke="white"
                          strokeWidth="1.5"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </svg>
                    )}
                  </span>
                  <span
                    className={`text-xs transition-colors ${valid ? 'text-gray-700' : 'text-gray-400'}`}
                  >
                    {label}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              Timezone
            </label>
            <input
              {...register('timezone', { required: 'Timezone is required' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg outline-none bg-gray-50 text-gray-600"
              readOnly
            />
            <p className="text-xs text-gray-500 mt-1">
              Detected automatically from your browser.
            </p>
            {errors.timezone && (
              <p className="text-sm text-red-500 mt-1">
                {errors.timezone.message}
              </p>
            )}
          </div>

          <Button
            type="submit"
            isLoading={isSubmitting}
            className="w-full bg-gray-900 hover:bg-gray-800 text-white py-2.5 rounded-lg font-semibold tracking-widest uppercase text-sm transition"
          >
            Sign Me Up!
          </Button>

          <p className="text-center text-sm text-gray-600">
            Already have an account?{' '}
            <Link
              to="/login"
              className="text-gray-900 font-semibold border-b border-gray-900 hover:opacity-70 transition-opacity"
            >
              Log in
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
};
