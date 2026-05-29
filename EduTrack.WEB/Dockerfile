FROM node:lts-alpine AS build
WORKDIR /app
COPY . .
RUN yarn install
RUN yarn build:staging

FROM nginx:alpine AS serve
COPY --from=build /app/dist/order-debt.web-staging/browser /usr/share/nginx/html
EXPOSE 80
