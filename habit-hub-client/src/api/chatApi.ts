import axios from 'axios';
import type { ChatMessage, MessageResponseDto, SendMessageDto } from '../types/chatTypes';
import api from './axiosInstance';

const mapMessageDto = (dto: MessageResponseDto): ChatMessage => ({
  id: dto.id,
  senderId: dto.senderId,
  senderName: dto.senderName,
  content: dto.content,
  sentAt: new Date(dto.sentAt),
});

export const getTeamMessages = async (teamId: string): Promise<ChatMessage[]> => {
  try {
    const response = await api.get<MessageResponseDto[]>(`/teams/${teamId}/chat/messages`);
    return response.data.map(mapMessageDto);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData && typeof serverData === 'object' && 'error' in serverData
            ? String(serverData.error)
            : null;

      throw new Error(
        serverMessage ||
          (status
            ? `Failed to load chat messages (HTTP ${status}).`
            : 'Failed to load chat messages. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while loading chat messages.');
  }
};

export const sendTeamMessage = async (
  teamId: string,
  dto: SendMessageDto
): Promise<ChatMessage> => {
  try {
    const response = await api.post<MessageResponseDto>(`/teams/${teamId}/chat/messages`, dto);
    return mapMessageDto(response.data);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData && typeof serverData === 'object' && 'error' in serverData
            ? String(serverData.error)
            : null;

      throw new Error(
        serverMessage ||
          (status
            ? `Failed to send message (HTTP ${status}).`
            : 'Failed to send message. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while sending message.');
  }
};

export const deleteTeamMessage = async (teamId: string, messageId: string): Promise<void> => {
  try {
    await api.delete(`/teams/${teamId}/chat/messages/${messageId}`);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData && typeof serverData === 'object' && 'error' in serverData
            ? String(serverData.error)
            : null;

      throw new Error(
        serverMessage ||
          (status
            ? `Failed to delete message (HTTP ${status}).`
            : 'Failed to delete message. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while deleting message.');
  }
};
