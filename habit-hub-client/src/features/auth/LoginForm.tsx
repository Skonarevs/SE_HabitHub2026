import { useForm } from 'react-hook-form';
import api from '../../api/axiosInstance';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';

interface LoginFormInputs {
  email: string;
  password: string;
  userType: 'Creator' | 'Member'; // added
}

export const LoginForm: React.FC = () => {
  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormInputs>({
    defaultValues: { // added
      userType: 'Member', // added
    } // added
  });
  const navigate = useNavigate();

  const onSubmit = async (data: LoginFormInputs) => {
    try {
      const response = await api.post('/auth/login', data);
      localStorage.setItem('sessionId', response.data.sessionId);
      navigate('/dashboard');
    } catch (error) {
      console.error("Login error", error);
    
    }
  }

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
      
     
      <form 
        onSubmit={handleSubmit(onSubmit)} 
        className="bg-white max-w-sm w-full rounded-2xl shadow-xl p-8 flex flex-col space-y-5"
      >
        <h2 className="text-2xl font-bold text-center text-gray-900 mb-4">HabitHub</h2>

        {/* Email */}
        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">Email</label>
          <input 
            {...register("email", { 
              required: "Email is required", 
              pattern: { value: /^\S+@\S+$/i, message: "Incorrect email format" } 
            })} 
            className={`w-full px-4 py-2 border rounded-lg outline-none transition-colors ${
              errors.email 
                ? 'border-red-500 focus:ring-2 focus:ring-red-200' 
                : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
            }`}
            placeholder="mail@example.com"
          />
          {errors.email && <p className="text-sm text-red-500 mt-1">{errors.email.message}</p>}
        </div>

        {/* Password */}
        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">Password</label>
          <input 
            type="password"
            {...register("password", { required: "Enter the password" })} 
            className={`w-full px-4 py-2 border rounded-lg outline-none transition-colors ${
              errors.password 
                ? 'border-red-500 focus:ring-2 focus:ring-red-200' 
                : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
            }`}
           
          />
          {errors.password && <p className="text-sm text-red-500 mt-1">{errors.password.message}</p>}
        </div>

        <div className="flex flex-col"> {/* added */}
          <label className="text-sm font-medium text-gray-700 mb-1">I am logging in as</label> {/* added */}
          <select
            {...register("userType", { required: "Select a role" })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 bg-white"
          >
            <option value="Member">Member</option>
            <option value="Creator">Creator</option>
          </select>
          {errors.userType && <p className="text-sm text-red-500 mt-1">{errors.userType.message}</p>}
        </div>

        {/* Button */}
        <button 
          type="submit" 
          className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-medium transition mt-2"
        >
          Login
        </button>
        <p className="text-center text-sm text-gray-600 mt-4">
          Don't have an account?{' '}
          <Link to="/register" className="text-blue-600 hover:text-blue-700 font-medium hover:underline transition-colors">
            Sign up
          </Link>
        </p>
      </form>

    </div>
  );
}