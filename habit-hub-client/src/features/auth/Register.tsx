import { useForm } from 'react-hook-form';
import api from '../../api/axiosInstance';
import { useNavigate, Link } from 'react-router-dom';

interface RegisterFormInputs { // added
  name: string; // added
  email: string; // added
  password: string; // added
  timezone: string; // added
  userType: 'Creator' | 'Member'; // added
} // added

export const Register: React.FC = () => { // slight reformating
  const detectedTimezone = Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC'; // added
  const { register, handleSubmit, formState: { errors } } = useForm<RegisterFormInputs>({ // added
    defaultValues: { // added
      timezone: detectedTimezone, // added
      userType: 'Member', // added
    }, // added
  }); // added
  const navigate = useNavigate();

  const onSubmit = async (data: RegisterFormInputs) => { // added RegisterFormInputs insead of any
    try {
      const response = await api.post<{ sessionId: string }>('/auth/register', data); // added respone type
      localStorage.setItem('sessionId', response.data.sessionId);
      navigate('/dashboard');
    } catch (error) {
      console.error("Registration error", error);
    }
  }

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
      <form 
        onSubmit={handleSubmit(onSubmit)} 
        className="bg-white max-w-sm w-full rounded-2xl shadow-xl p-8 flex flex-col space-y-5"
      >
        <h2 className="text-2xl font-bold text-center text-gray-900 mb-2">HabitHub</h2>
        <p className="text-center text-gray-500 text-sm mb-4">Create an account</p>

         {/* added */}
        <div className="flex flex-col"> 
          <label className="text-sm font-medium text-gray-700 mb-1">Name</label>
          <input
            {...register("name", {
              required: "Name is required",
              minLength: { value: 2, message: "Minimum 2 characters" }
            })}
            className={`w-full px-4 py-2 border rounded-lg outline-none transition-colors ${
              errors.name
                ? 'border-red-500 focus:ring-2 focus:ring-red-200'
                : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
            }`}
            placeholder="Your name"
          />
          {errors.name && <p className="text-sm text-red-500 mt-1">{errors.name.message}</p>}
        </div>


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
          {errors.email && <p className="text-sm text-red-500 mt-1">{errors.email.message?.toString()}</p>}
        </div>

        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">I want to...</label>
          <select 
            {...register("userType", { required: "Select a role" })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 bg-white"
          >
            <option value="Member">Join a team</option>
            <option value="Creator">Create my own team</option>
          </select>
        </div>

        {/* added */}
        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">Timezone</label>
          <input
            {...register("timezone", { required: "Timezone is required" })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg outline-none bg-gray-50 text-gray-600"
            readOnly
          />
          <p className="text-xs text-gray-500 mt-1">Detected automatically from your browser.</p>
          {errors.timezone && <p className="text-sm text-red-500 mt-1">{errors.timezone.message}</p>}
        </div>

        <div className="flex flex-col">
          <label className="text-sm font-medium text-gray-700 mb-1">Password</label>
          <input 
            type="password"
            {...register("password", { 
              required: "Enter the password",
              minLength: { value: 8, message: "Minimum 8 characters" } 
            })} 
            className={`w-full px-4 py-2 border rounded-lg outline-none transition-colors ${
              errors.password 
                ? 'border-red-500 focus:ring-2 focus:ring-red-200' 
                : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
            }`}
          />
          {errors.password && <p className="text-sm text-red-500 mt-1">{errors.password.message?.toString()}</p>}
        </div>

        <button 
          type="submit" 
          className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-medium transition mt-2"
        >
          Sign Up
        </button>

        <p className="text-center text-sm text-gray-600 mt-4">
          Already have an account?{' '}
          <Link to="/login" className="text-blue-600 hover:underline">Login</Link>
        </p>
      </form>
    </div>
  );
}