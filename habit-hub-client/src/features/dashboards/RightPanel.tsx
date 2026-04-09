export const RightPanel = () => {
  return (
    <aside className="flex flex-col pr-5 py-8 justify-between h-full pl-3 border border-gray-100 rounded-3xl shadow-sm">
      <h3 className="text-center text-gray-500 text-sm font-medium border-b border-gray-100 pb-2 mb-4">
        Activity
      </h3>
      <div className="flex-1 overflow-y-auto pr-2 space-y-4">
        <div className="bg-gray-100 text-black p-4 rounded-2xl rounded-tl-none text-sm">
          Hi! Next week we'll start a new habit. I'll tell you all the details
          later
        </div>
      </div>

      <div className="mt-4 pt-2">
        <div className="bg-gray-50 rounded-full flex items-center px-4 py-3">
          <input
            type="text"
            placeholder="Write a message"
            className="bg-transparent w-full outline-none text-sm"
          />
        </div>
      </div>
    </aside>
  );
};
