NAME=rabbitmq-dotnet-client
NAME_VSN=${NAME}-${RABBIT_VSN}

RELEASE_DIR=releases/${NAME}/v${RABBIT_VSN}

STAGE_RELEASE_DIR=charlotte:/home/rabbitmq/stage-extras/releases/${NAME}
LIVE_RELEASE_DIR=charlotte:/home/rabbitmq/live-extras/releases/${NAME}

RSYNC_CMD=rsync -irvl --delete-after

TMPXMLZIP=${NAME_VSN}-tmp-xmldoc.zip

ifeq "$(RABBIT_VSN)" ""
rabbit-vsn:
	@echo "RABBIT_VSN is not set"
	@false
else
rabbit-vsn: 
endif

deploy-stage: rabbit-vsn ensure-deliverables
	${RSYNC_CMD} --exclude=${TMPXMLZIP} releases/${NAME}/ ${STAGE_RELEASE_DIR}

deploy-live: rabbit-vsn ensure-deliverables
	${RSYNC_CMD} --exclude=${TMPXMLZIP} releases/${NAME}/ ${LIVE_RELEASE_DIR}

ensure-deliverables: rabbit-vsn
	file ${RELEASE_DIR}/${NAME_VSN}.zip
	file ${RELEASE_DIR}/${NAME_VSN}-api-guide.pdf
	file ${RELEASE_DIR}/${NAME_VSN}-user-guide.pdf
	file ${RELEASE_DIR}/${NAME_VSN}-wcf-service-model.pdf
	file ${RELEASE_DIR}/${NAME_VSN}-net-2.0.zip
	file ${RELEASE_DIR}/${NAME_VSN}-net-2.0-htmldoc.zip
	file ${RELEASE_DIR}/${NAME_VSN}-net-2.0-htmldoc
	file ${RELEASE_DIR}/${NAME_VSN}-net-3.0-wcf.zip
	file ${RELEASE_DIR}/${NAME_VSN}-net-3.0-wcf-htmldoc.zip
	file ${RELEASE_DIR}/${NAME_VSN}-net-3.0-wcf-htmldoc

ensure-prerequisites: rabbit-vsn
	dpkg -p htmldoc plotutils transfig graphviz > /dev/null

ensure-release-dir: rabbit-vsn
	touch ${RELEASE_DIR}/

ensure-docs: rabbit-vsn
	file ${RELEASE_DIR}/${NAME_VSN}-net-2.0-htmldoc.zip
	file ${RELEASE_DIR}/${TMPXMLZIP}

doc: rabbit-vsn ensure-prerequisites ensure-release-dir ensure-docs
	rm -rf build/tmpdoc build/doc
	mkdir -p build/tmpdoc/html build/tmpdoc/xml
	unzip -j ${RELEASE_DIR}/${NAME_VSN}-net-2.0-htmldoc.zip -d build/tmpdoc/html
	unzip -j ${RELEASE_DIR}/${NAME_VSN}-tmp-xmldoc.zip -d build/tmpdoc/xml
	cd docs && ./api-guide.sh && \
	  mv api-guide.pdf ../${RELEASE_DIR}/${NAME_VSN}-api-guide.pdf
	$(MAKE) -C docs
	mv build/doc/userguide/user-guide.pdf ${RELEASE_DIR}/${NAME_VSN}-user-guide.pdf
	cp docs/"RabbitMQ Service Model.pdf" \
	  ${RELEASE_DIR}/${NAME_VSN}-wcf-service-model.pdf
	cd ${RELEASE_DIR} && \
	  rm -rf ${NAME_VSN}-net-2.0-htmldoc && \
	  unzip ${NAME_VSN}-net-2.0-htmldoc.zip && \
	  rm -rf unzip ${NAME_VSN}-net-3.0-wcf-htmldoc && \
	  unzip ${NAME_VSN}-net-3.0-wcf-htmldoc.zip
