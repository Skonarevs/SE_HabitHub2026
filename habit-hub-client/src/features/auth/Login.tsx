import { useForm } from 'react-hook-form';
import api from '../../api/axiosInstance';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAuthStore } from '../../store/useAuthStore';
import { Button } from '../../components/ui/Button';

interface LoginInputs {
  email: string;
  password: string;
  userType: 'Creator' | 'Member';
}

export const Login: React.FC = () => {
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
      login(response.data.name, response.data.sessionId);
      navigate('/dashboard');
    } catch (error: any) {
      if (!error.response) {
        toast.error('Server is not responding. Please try again later.');
        return;
      }

      const status = Number(error.response.status);
      const serverMessage =
        error.response.data?.error || 'Something went wrong';

      if (status === 401 || status === 404) {
        setError('email', {
          type: 'server',
          message: serverMessage,
        });

        setError('password', {
          type: 'server',
          message: serverMessage,
        });
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
      <div className="absolute inset-0 bg-black/10"></div>
      <div className="relative w-full flex items-center justify-between px-20">
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="bg-white/90 backdrop-blur-md max-w-md w-full h-auto rounded-2xl shadow-2xl p-12 flex flex-col space-y-6"
        >
          <h2 className="text-2xl font-bold text-center text-gray-900 mb-4">
            HabitHub
          </h2>

          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <input
              {...register('email', {
                required: 'Email is required',
                pattern: {
                  value: /^\S+@\S+$/i,
                  message: 'Incorrect email format',
                },
              })}
              className={`w-full px-4 py-2 border rounded-lg outline-none transition-colors ${
                errors.email
                  ? 'border-red-500 focus:ring-2 focus:ring-red-200'
                  : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
              }`}
              placeholder="mail@example.com"
            />
            {errors.email && (
              <p className="text-sm text-red-500 mt-1">
                {errors.email.message}
              </p>
            )}
          </div>

          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              Password
            </label>
            <input
              type="password"
              {...register('password', { required: 'Enter the password' })}
              className={`w-full px-4 py-2 border rounded-lg outline-none transition-colors ${
                errors.password
                  ? 'border-red-500 focus:ring-2 focus:ring-red-200'
                  : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
              } minLength: { value: 8, message: "Minimum 8 characters" }`}
            />
            {errors.password && (
              <p className="text-sm text-red-500 mt-1">
                {errors.password.message}
              </p>
            )}
          </div>

          <div className="flex flex-col">
            <label className="text-sm font-medium text-gray-700 mb-1">
              I am logging in as
            </label>
            <select
              {...register('userType', { required: 'Select a role' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 bg-white"
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

          <Button
            type="submit"
            isLoading={isSubmitting}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-medium transition"
          >
            Login
          </Button>
          <p className="text-center text-sm text-gray-600 mt-4">
            Don't have an account?{' '}
            <Link
              to="/register"
              className="text-blue-600 hover:text-blue-700 font-medium hover:underline transition-colors"
            >
              Sign up
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
};
