import React from 'react';
import { Spinner } from './Spinner';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  isLoading?: boolean;
  children: React.ReactNode;
}

export const Button: React.FC<ButtonProps> = ({
  isLoading,
  children,
  ...props
}) => {
  return (
    <button
      {...props}
      disabled={isLoading || props.disabled}
      className={`flex items-center justify-center px-4 py-2 rounded transition ${
        isLoading ? 'opacity-70 cursor-not-allowed' : ''
      } ${props.className || ''}`}
    >
      {isLoading ? (
        <>
          <Spinner />
          <span className="ml-2">Loading...</span>
        </>
      ) : (
        children
      )}
    </button>
  );
};


