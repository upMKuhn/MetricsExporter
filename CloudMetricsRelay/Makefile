WEB_PORT=8002
up:
	docker-compose down
	docker-compose up --build -d
	@echo -e "\n\n\n\n\n\n\n\n\n\n"
	@echo -e "Metrics Relay should be up by now ;) \n"
	@echo -e "Push metrics via websocket chrome plugin to ws://localhost:${WEB_PORT}/metrics or curl :P"
	@echo -e "test data is ./resources/sample_br_metric.json"
	@echo "http://localhost:${WEB_PORT}/BR_Metrics/show-metrics"

down:
	docker-compose down

tail:
	docker-compose logs -f

send-tick:
	cat ./ressources/sample_br_metric.json | tr '\n' ' '  | websocat -B 6553600 ws://localhost:${WEB_PORT}/metrics

show-br:
	xdg-open "http://localhost:${WEB_PORT}/BR_Metrics/show-metrics"