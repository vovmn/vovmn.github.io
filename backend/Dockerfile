FROM node:20-alpine
WORKDIR /usr/src/app
COPY package*.json ./
RUN npm ci --omit=dev --no-audit --no-fund --fetch-timeout=60000
COPY . .
ENV NODE_ENV=production
EXPOSE 5000
CMD ["npm","start"]
