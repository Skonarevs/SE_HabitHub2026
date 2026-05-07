export interface SendMessageDto {
  content: string;
}

export interface MessageResponseDto {
  id: string;
  senderId: string;
  senderName: string;
  content: string;
  sentAt: string;
}

export interface ChatMessage {
  id: string;
  senderId: string;
  senderName: string;
  content: string;
  sentAt: Date;
}
