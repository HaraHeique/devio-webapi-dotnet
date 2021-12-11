using AutoMapper;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repositories;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers.V2
{
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/fornecedores")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorService _fornecedorService;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IMapper _mapper;

        public FornecedoresController(IFornecedorService fornecedorService,
                                      IFornecedorRepository fornecedorRepository,
                                      IEnderecoRepository enderecoRepository,
                                      IMapper mapper,
                                      INotificador notificador) : base(notificador)
        {
            _fornecedorService = fornecedorService;
            _fornecedorRepository = fornecedorRepository;
            _enderecoRepository = enderecoRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FornecedorViewModel>>> ObterTodos()
        {
            var fornecedoresVM = _mapper.Map<IEnumerable<FornecedorViewModel>>(await _fornecedorRepository.ObterTodos());

            return CustomResponse(fornecedoresVM);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> ObterPorId([FromRoute] Guid id)
        {
            var fornecedorVM = await ObterFornecedorProdutosEndereco(id);

            if (fornecedorVM == null) return NotFound("Fornecedor não encontrado.");

            return CustomResponse(fornecedorVM);
        }

        [HttpGet("obter-endereco/{id:guid}")]
        public async Task<ActionResult<EnderecoViewModel>> ObterEnderecoPorId([FromRoute] Guid id)
        {
            var enderecoVM = _mapper.Map<EnderecoViewModel>(await _enderecoRepository.ObterEnderecoPorFornecedor(id));

            return CustomResponse(enderecoVM);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        //[ClaimsAuthorize("Fornecedor", "Adicionar")]
        public async Task<ActionResult<FornecedorViewModel>> Adicionar([FromBody] FornecedorViewModel fornecedorVM)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorVM);

            await _fornecedorService.Adicionar(fornecedor);

            return CustomResponse(_mapper.Map<FornecedorViewModel>(fornecedor));
        }

        [HttpPut("{id:guid}")]
        //[ClaimsAuthorize("Fornecedor", "Atualizar")]
        public async Task<ActionResult<FornecedorViewModel>> Atualizar([FromRoute] Guid id, [FromBody] FornecedorViewModel fornecedorVM)
        {
            if (id != fornecedorVM.Id) return CustomErrorResponse("O ID informado não é o mesmo que foi passado na rota!");

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (await _fornecedorRepository.Buscar(p => p.Id == id) == null) return NotFound("O fornecedor não foi encontrado.");

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorVM);

            await _fornecedorService.Atualizar(fornecedor);

            return CustomResponse(fornecedorVM);
        }

        [HttpPut("atualizar-endereco/{id:guid}")]
        //[ClaimsAuthorize("Fornecedor", "Atualizar")]
        public async Task<ActionResult<EnderecoViewModel>> AtualizarEndereco([FromRoute] Guid id, [FromBody] EnderecoViewModel enderecoVM)
        {
            if (id != enderecoVM.Id) return CustomErrorResponse("O ID informado não é o mesmo que foi passado na rota!");

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            //if (await _enderecoRepository.Buscar(e => e.Id == id) == null) return NotFound();

            var endereco = _mapper.Map<Endereco>(enderecoVM);

            await _fornecedorService.AtualizarEndereco(endereco);

            return CustomResponse(enderecoVM);
        }

        [HttpDelete("{id:guid}")]
        //[ClaimsAuthorize("Fornecedor", "Excluir")]
        public async Task<IActionResult> Remover([FromRoute] Guid id)
        {
            var fornecedorVM = await ObterFornecedorEndereco(id);

            if (fornecedorVM == null) return NotFound("Fornecedor não encontrado.");

            await _fornecedorService.Remover(id);

            return CustomResponse();
        }

        private async Task<FornecedorViewModel> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorViewModel>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
        }

        private async Task<FornecedorViewModel> ObterFornecedorEndereco(Guid id)
        {
            return _mapper.Map<FornecedorViewModel>(await _fornecedorRepository.ObterFornecedorEndereco(id));
        }
    }
}
