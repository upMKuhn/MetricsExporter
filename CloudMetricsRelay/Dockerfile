FROM node:alpine

WORKDIR /app
COPY . .
WORKDIR /app/src
RUN npm install
RUN chmod +x -R ./
CMD ["--experimental-modules","index.mjs"]
EXPOSE 80